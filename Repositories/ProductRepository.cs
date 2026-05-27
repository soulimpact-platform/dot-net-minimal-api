using Npgsql;
using NpgsqlTypes;

// productsテーブルへのデータアクセスを行うRepository
public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        // PostgreSQLへの接続文字列を取得
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured");
    }

    public ProductSearchResultResponse Search(
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
        var products = new List<ProductResponse>();

        // ソート対象、並び順、取得開始位置を設定
        var orderBy = GetOrderBy(sortBy);
        var orderDirection = GetOrderDirection(sortOrder);
        var offset = (page - 1) * pageSize;

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 検索条件に一致する総件数を取得
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = $@"
            SELECT COUNT(*)
            {BuildFromWhereQuery()}
        ";

        // 検索条件をSQLパラメータに設定
        AddSearchParameters(countCommand, name, category, author, minPrice, maxPrice);

        var totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

        // 検索結果をページ単位で取得
        var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT
                p.id AS id,
                p.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                p.price AS price
            {BuildBaseQuery(orderBy, orderDirection)}
            LIMIT @pageSize OFFSET @offset
        ";

        // 検索条件をSQLパラメータに設定
        AddSearchParameters(command, name, category, author, minPrice, maxPrice);

        // ページング用の取得件数と取得開始位置を設定
        command.Parameters.AddWithValue("pageSize", pageSize);
        command.Parameters.AddWithValue("offset", offset);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // DBから取得した行を検索結果用レスポンスに変換
            products.Add(CreateProductResponse(reader));
        }

        // 検索結果、総件数、ページ情報を返却
        return new ProductSearchResultResponse(
            products,
            totalCount,
            page,
            pageSize
        );
    }

    public List<ProductResponse> SearchAll(
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
        var products = new List<ProductResponse>();

        // CSV出力用のソート対象と並び順を設定
        var orderBy = GetOrderBy(sortBy);
        var orderDirection = GetOrderDirection(sortOrder);

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 検索条件に一致する書籍を指定件数まで取得
        var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT
                p.id AS id,
                p.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                p.price AS price
            {BuildBaseQuery(orderBy, orderDirection)}
            LIMIT @limit
        ";

        // 検索条件をSQLパラメータに設定
        AddSearchParameters(command, name, category, author, minPrice, maxPrice);

        // CSV出力時の最大取得件数を設定
        command.Parameters.AddWithValue("limit", limit);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // DBから取得した行をCSV出力用レスポンスに変換
            products.Add(CreateProductResponse(reader));
        }

        return products;
    }

    public List<ProductResponse> FindAll()
    {
        var products = new List<ProductResponse>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 管理者向けに書籍一覧を取得
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                p.id AS id,
                p.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                p.price AS price
            FROM products p
            INNER JOIN categories c ON p.category_id = c.id
            INNER JOIN authors a ON p.author_id = a.id
            ORDER BY p.id ASC
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            products.Add(CreateProductResponse(reader));
        }

        return products;
    }

    public ProductDetailResponse? FindById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 指定されたIDの書籍詳細を取得
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                p.id AS id,
                p.name AS name,
                c.name AS category_name,
                a.name AS author_name,
                p.price AS price,
                p.description AS description
            FROM products p
            INNER JOIN categories c ON p.category_id = c.id
            INNER JOIN authors a ON p.author_id = a.id
            WHERE p.id = @id
        ";

        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        // DBから取得した行を書籍詳細用レスポンスに変換
        return CreateProductDetailResponse(reader);
    }

    public void Create(string name, string category, string author, int price, string description)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // カテゴリ・著者を取得し、存在しない場合は追加
            var categoryId = GetOrCreateCategoryId(connection, transaction, category);
            var authorId = GetOrCreateAuthorId(connection, transaction, author);

            // 書籍を追加
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT INTO products (name, category_id, author_id, price, description)
                VALUES (@name, @categoryId, @authorId, @price, @description)
            ";

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("categoryId", categoryId);
            command.Parameters.AddWithValue("authorId", authorId);
            command.Parameters.AddWithValue("price", price);
            command.Parameters.AddWithValue("description", description);

            command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Update(int id, string name, string category, string author, int price, string description)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            // カテゴリ・著者を取得し、存在しない場合は追加
            var categoryId = GetOrCreateCategoryId(connection, transaction, category);
            var authorId = GetOrCreateAuthorId(connection, transaction, author);

            // 書籍を更新
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                UPDATE products
                SET name = @name,
                    category_id = @categoryId,
                    author_id = @authorId,
                    price = @price,
                    description = @description
                WHERE id = @id
            ";

            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("categoryId", categoryId);
            command.Parameters.AddWithValue("authorId", authorId);
            command.Parameters.AddWithValue("price", price);
            command.Parameters.AddWithValue("description", description);

            command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Delete(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 書籍を削除
        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM products
            WHERE id = @id
        ";

        command.Parameters.AddWithValue("id", id);

        command.ExecuteNonQuery();
    }

    // DBから取得した行を書籍検索結果用レスポンスに変換
    private static ProductResponse CreateProductResponse(NpgsqlDataReader reader)
    {
        return new ProductResponse(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("category_name")),
            reader.GetString(reader.GetOrdinal("author_name")),
            reader.GetInt32(reader.GetOrdinal("price"))
        );
    }

    // DBから取得した行を書籍詳細用レスポンスに変換
    private static ProductDetailResponse CreateProductDetailResponse(NpgsqlDataReader reader)
    {
        return new ProductDetailResponse(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("category_name")),
            reader.GetString(reader.GetOrdinal("author_name")),
            reader.GetInt32(reader.GetOrdinal("price")),
            reader.GetString(reader.GetOrdinal("description"))
        );
    }

    // 書籍検索で共通利用するFROM句とWHERE句を取得
    private static string BuildFromWhereQuery()
    {
        return @"
            FROM products p
            INNER JOIN categories c ON p.category_id = c.id
            INNER JOIN authors a ON p.author_id = a.id
            WHERE (@name = '' OR p.name LIKE @nameLike ESCAPE '!')
              AND (@category = '' OR c.name LIKE @categoryLike ESCAPE '!')
              AND (@author = '' OR a.name LIKE @authorLike ESCAPE '!')
              AND (@minPrice IS NULL OR p.price >= @minPrice)
              AND (@maxPrice IS NULL OR p.price <= @maxPrice)
        ";
    }

    // 書籍検索で共通利用するFROM句、WHERE句、ORDER BY句を取得
    private static string BuildBaseQuery(string orderBy, string orderDirection)
    {
        return $@"
            {BuildFromWhereQuery()}
            ORDER BY {orderBy} {orderDirection}, p.id ASC
        ";
    }

    // 書籍検索で共通利用するSQLパラメータを設定
    private static void AddSearchParameters(
        NpgsqlCommand command,
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice
    )
    {
        // LIKE検索用にワイルドカード文字をエスケープ
        var escapedName = EscapeLike(name);
        var escapedCategory = EscapeLike(category);
        var escapedAuthor = EscapeLike(author);

        // 文字列検索条件を設定
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("nameLike", $"%{escapedName}%");
        command.Parameters.AddWithValue("category", category);
        command.Parameters.AddWithValue("categoryLike", $"%{escapedCategory}%");
        command.Parameters.AddWithValue("author", author);
        command.Parameters.AddWithValue("authorLike", $"%{escapedAuthor}%");

        // 価格下限が未入力の場合もinteger型のNULLとして設定
        command.Parameters.Add("minPrice", NpgsqlDbType.Integer).Value =
            minPrice.HasValue ? minPrice.Value : DBNull.Value;

        // 価格上限が未入力の場合もinteger型のNULLとして設定
        command.Parameters.Add("maxPrice", NpgsqlDbType.Integer).Value =
            maxPrice.HasValue ? maxPrice.Value : DBNull.Value;
    }

    // カテゴリ名からカテゴリIDを取得し、存在しない場合は追加
    private static int GetOrCreateCategoryId(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string category
    )
    {
        var selectCommand = connection.CreateCommand();
        selectCommand.Transaction = transaction;
        selectCommand.CommandText = @"
            SELECT id
            FROM categories
            WHERE name = @name
        ";

        selectCommand.Parameters.AddWithValue("name", category);

        var existingId = selectCommand.ExecuteScalar();

        if (existingId is not null)
        {
            return Convert.ToInt32(existingId);
        }

        var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = @"
            INSERT INTO categories (id, name)
            VALUES ((SELECT COALESCE(MAX(id), 0) + 1 FROM categories), @name)
            RETURNING id
        ";

        insertCommand.Parameters.AddWithValue("name", category);

        return Convert.ToInt32(insertCommand.ExecuteScalar());
    }

    // 著者名から著者IDを取得し、存在しない場合は追加
    private static int GetOrCreateAuthorId(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string author
    )
    {
        var selectCommand = connection.CreateCommand();
        selectCommand.Transaction = transaction;
        selectCommand.CommandText = @"
            SELECT id
            FROM authors
            WHERE name = @name
        ";

        selectCommand.Parameters.AddWithValue("name", author);

        var existingId = selectCommand.ExecuteScalar();

        if (existingId is not null)
        {
            return Convert.ToInt32(existingId);
        }

        var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = @"
            INSERT INTO authors (id, name)
            VALUES ((SELECT COALESCE(MAX(id), 0) + 1 FROM authors), @name)
            RETURNING id
        ";

        insertCommand.Parameters.AddWithValue("name", author);

        return Convert.ToInt32(insertCommand.ExecuteScalar());
    }

    // LIKE検索用に特殊文字をエスケープ
    private static string EscapeLike(string value)
    {
        return value
            .Replace("!", "!!")
            .Replace("%", "!%")
            .Replace("_", "!_");
    }

    // 画面から指定された並び替え項目をSQLの列名に変換
    private static string GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "name" => "p.name",
            "category" => "c.name",
            _ => "p.id"
        };
    }

    // 画面から指定された並び順をSQLのASC/DESCに変換
    private static string GetOrderDirection(string sortOrder)
    {
        return string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";
    }
}