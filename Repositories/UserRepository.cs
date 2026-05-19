using Npgsql;

// usersテーブルへのデータアクセスを行うRepository
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        // PostgreSQLへの接続文字列を取得
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured");
    }

    public string? FindUsername(string username, string password)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT username
            FROM users
            WHERE username = @username
              AND password = @password
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("password", password);

        return command.ExecuteScalar() as string;
    }
}