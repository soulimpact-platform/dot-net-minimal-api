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

    public UserInfo? FindByUsernameAndPassword(string username, string password)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, username, role
            FROM users
            WHERE username = @username
              AND password = @password
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("password", password);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new UserInfo(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        );
    }
}