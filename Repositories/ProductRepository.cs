using Npgsql;

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

    public List<ProductResponse> Search(string name, string category)
    {
        var products = new List<ProductResponse>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, name, category, price
            FROM products
            WHERE (@name = '' OR name LIKE @nameLike)
              AND (@category = '' OR category LIKE @categoryLike)
            ORDER BY id
        ";

        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("nameLike", $"%{name}%");
        command.Parameters.AddWithValue("category", category);
        command.Parameters.AddWithValue("categoryLike", $"%{category}%");

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            products.Add(new ProductResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)
            ));
        }

        return products;
    }

    public ProductDetailResponse? FindById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, name, category, price, description
            FROM products
            WHERE id = @id
        ";

        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new ProductDetailResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4)
        );
    }
}