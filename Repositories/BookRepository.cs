using Npgsql;
using NpgsqlTypes;

// booksテーブルへのデータアクセスを行うRepository
public class BookRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured");
    }

    public BookSearchResultResponse Search(
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice,
        string sortBy,
        string sortOrder,
        int page,
        int pageSize
    )
    {
        var books = new List<BookResponse>();
        var orderBy = GetOrderBy(sortBy);
        var orderDirection = GetOrderDirection(sortOrder);
        var offset = (page - 1) * pageSize;

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = $@"
            SELECT COUNT(*)
            {BuildFromWhereQuery()}
        ";
        AddSearchParameters(countCommand, name, category, author, minPrice, maxPrice);

        var totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

        var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT
                b.id AS id,
                b.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                b.price AS price
            {BuildBaseQuery(orderBy, orderDirection)}
            LIMIT @pageSize OFFSET @offset
        ";
        AddSearchParameters(command, name, category, author, minPrice, maxPrice);
        command.Parameters.AddWithValue("pageSize", pageSize);
        command.Parameters.AddWithValue("offset", offset);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            books.Add(CreateBookResponse(reader));
        }

        return new BookSearchResultResponse(
            books,
            totalCount,
            page,
            pageSize
        );
    }

    public List<BookResponse> SearchAll(
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice,
        string sortBy,
        string sortOrder,
        int limit
    )
    {
        var books = new List<BookResponse>();
        var orderBy = GetOrderBy(sortBy);
        var orderDirection = GetOrderDirection(sortOrder);

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT
                b.id AS id,
                b.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                b.price AS price
            {BuildBaseQuery(orderBy, orderDirection)}
            LIMIT @limit
        ";
        AddSearchParameters(command, name, category, author, minPrice, maxPrice);
        command.Parameters.AddWithValue("limit", limit);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            books.Add(CreateBookResponse(reader));
        }

        return books;
    }

    public List<BookResponse> FindAll()
    {
        var books = new List<BookResponse>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                b.id AS id,
                b.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                b.price AS price
            FROM books b
            INNER JOIN categories c
                ON b.category_id = c.id
               AND c.deleted_at IS NULL
            INNER JOIN book_authors a
                ON b.author_id = a.id
               AND a.deleted_at IS NULL
            WHERE b.deleted_at IS NULL
            ORDER BY b.id ASC
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            books.Add(CreateBookResponse(reader));
        }

        return books;
    }

    public BookDetailResponse? FindById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                b.id AS id,
                b.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                b.price AS price,
                b.description AS description
            FROM books b
            INNER JOIN categories c
                ON b.category_id = c.id
               AND c.deleted_at IS NULL
            INNER JOIN book_authors a
                ON b.author_id = a.id
               AND a.deleted_at IS NULL
            WHERE b.id = @id
              AND b.deleted_at IS NULL
        ";

        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return CreateBookDetailResponse(reader);
    }

    public void Create(string name, string category, string author, int price, string description, string createdBy)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var categoryId = GetOrCreateCategoryId(connection, transaction, category, createdBy);
        var authorId = GetOrCreateAuthorId(connection, transaction, author, createdBy);

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO books (
                name,
                category_id,
                author_id,
                price,
                description,
                created_by,
                updated_by
            )
            VALUES (
                @name,
                @categoryId,
                @authorId,
                @price,
                @description,
                @createdBy,
                @updatedBy
            )
        ";

        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("categoryId", categoryId);
        command.Parameters.AddWithValue("authorId", authorId);
        command.Parameters.AddWithValue("price", price);
        command.Parameters.AddWithValue("description", description);
        command.Parameters.AddWithValue("createdBy", createdBy);
        command.Parameters.AddWithValue("updatedBy", createdBy);

        command.ExecuteNonQuery();

        transaction.Commit();
    }

    public void Update(int id, string name, string category, string author, int price, string description, string updatedBy)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var categoryId = GetOrCreateCategoryId(connection, transaction, category, updatedBy);
        var authorId = GetOrCreateAuthorId(connection, transaction, author, updatedBy);

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE books
            SET name = @name,
                category_id = @categoryId,
                author_id = @authorId,
                price = @price,
                description = @description,
                updated_by = @updatedBy
            WHERE id = @id
              AND deleted_at IS NULL
        ";

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("categoryId", categoryId);
        command.Parameters.AddWithValue("authorId", authorId);
        command.Parameters.AddWithValue("price", price);
        command.Parameters.AddWithValue("description", description);
        command.Parameters.AddWithValue("updatedBy", updatedBy);

        command.ExecuteNonQuery();

        transaction.Commit();
    }

    public void Delete(int id, string updatedBy)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE books
            SET deleted_at = CURRENT_TIMESTAMP,
                updated_by = @updatedBy
            WHERE id = @id
              AND deleted_at IS NULL
        ";

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("updatedBy", updatedBy);

        command.ExecuteNonQuery();
    }

    private static BookResponse CreateBookResponse(NpgsqlDataReader reader)
    {
        return new BookResponse(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("category_name")),
            reader.GetString(reader.GetOrdinal("author_name")),
            reader.GetInt32(reader.GetOrdinal("price"))
        );
    }

    private static BookDetailResponse CreateBookDetailResponse(NpgsqlDataReader reader)
    {
        return new BookDetailResponse(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("category_name")),
            reader.GetString(reader.GetOrdinal("author_name")),
            reader.GetInt32(reader.GetOrdinal("price")),
            reader.GetString(reader.GetOrdinal("description"))
        );
    }

    private static string BuildFromWhereQuery()
    {
        return @"
            FROM books b
            INNER JOIN categories c
                ON b.category_id = c.id
               AND c.deleted_at IS NULL
            INNER JOIN book_authors a
                ON b.author_id = a.id
               AND a.deleted_at IS NULL
            WHERE b.deleted_at IS NULL
              AND (@name = '' OR b.name LIKE @nameLike ESCAPE '!')
              AND (@category = '' OR c.name LIKE @categoryLike ESCAPE '!')
              AND (@author = '' OR a.name LIKE @authorLike ESCAPE '!')
              AND (@minPrice IS NULL OR b.price >= @minPrice)
              AND (@maxPrice IS NULL OR b.price <= @maxPrice)
        ";
    }

    private static string BuildBaseQuery(string orderBy, string orderDirection)
    {
        return $@"
            {BuildFromWhereQuery()}
            ORDER BY {orderBy} {orderDirection}, b.id ASC
        ";
    }

    private static void AddSearchParameters(
        NpgsqlCommand command,
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice
    )
    {
        var escapedName = EscapeLike(name);
        var escapedCategory = EscapeLike(category);
        var escapedAuthor = EscapeLike(author);

        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("nameLike", $"%{escapedName}%");
        command.Parameters.AddWithValue("category", category);
        command.Parameters.AddWithValue("categoryLike", $"%{escapedCategory}%");
        command.Parameters.AddWithValue("author", author);
        command.Parameters.AddWithValue("authorLike", $"%{escapedAuthor}%");

        command.Parameters.Add("minPrice", NpgsqlDbType.Integer).Value =
            minPrice.HasValue ? minPrice.Value : DBNull.Value;

        command.Parameters.Add("maxPrice", NpgsqlDbType.Integer).Value =
            maxPrice.HasValue ? maxPrice.Value : DBNull.Value;
    }

    private static int GetOrCreateCategoryId(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string category,
        string username
    )
    {
        var existingId = FindActiveIdByName(
            connection,
            transaction,
            "categories",
            category
        );

        if (existingId is not null)
        {
            return existingId.Value;
        }

        return InsertNameAndReturnId(
            connection,
            transaction,
            "categories",
            category,
            username
        );
    }

    private static int GetOrCreateAuthorId(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string author,
        string username
    )
    {
        var existingId = FindActiveIdByName(
            connection,
            transaction,
            "book_authors",
            author
        );

        if (existingId is not null)
        {
            return existingId.Value;
        }

        return InsertNameAndReturnId(
            connection,
            transaction,
            "book_authors",
            author,
            username
        );
    }

    private static int? FindActiveIdByName(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        string name
    )
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $@"
            SELECT id
            FROM {tableName}
            WHERE name = @name
              AND deleted_at IS NULL
        ";

        command.Parameters.AddWithValue("name", name);

        var result = command.ExecuteScalar();

        return result is null ? null : Convert.ToInt32(result);
    }

    private static int InsertNameAndReturnId(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string tableName,
        string name,
        string username
    )
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $@"
            INSERT INTO {tableName} (name, created_by, updated_by)
            VALUES (@name, @createdBy, @updatedBy)
            ON CONFLICT DO NOTHING
            RETURNING id
        ";

        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("createdBy", username);
        command.Parameters.AddWithValue("updatedBy", username);

        var insertedId = command.ExecuteScalar();

        if (insertedId is not null)
        {
            return Convert.ToInt32(insertedId);
        }

        var existingId = FindActiveIdByName(
            connection,
            transaction,
            tableName,
            name
        );

        if (existingId is null)
        {
            throw new InvalidOperationException("Failed to resolve related master data.");
        }

        return existingId.Value;
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace("!", "!!")
            .Replace("%", "!%")
            .Replace("_", "!_");
    }

    private static string GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "name" => "b.name",
            "category" => "c.name",
            _ => "b.id"
        };
    }

    private static string GetOrderDirection(string sortOrder)
    {
        return string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";
    }
}
