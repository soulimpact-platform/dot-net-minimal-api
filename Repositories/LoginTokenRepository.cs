using Npgsql;

// login_tokensテーブルへのデータアクセスを行うRepository
public class LoginTokenRepository : ILoginTokenRepository
{
    private readonly string _connectionString;

    public LoginTokenRepository(IConfiguration configuration)
    {
        // PostgreSQLへの接続文字列を取得
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured");
    }

    public void Save(string username, string token, DateTime expiresAt)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO login_tokens (username, token, expires_at)
            VALUES (@username, @token, @expiresAt);
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("expiresAt", expiresAt);

        command.ExecuteNonQuery();
    }

    public bool Exists(string username, string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM login_tokens
            WHERE username = @username
              AND token = @token
              AND expires_at > NOW()
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("token", token);

        var count = (long)command.ExecuteScalar()!;

        return count > 0;
    }

    public void Delete(string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM login_tokens
            WHERE token = @token
        ";

        command.Parameters.AddWithValue("token", token);

        command.ExecuteNonQuery();
    }

    public void DeleteExpired(string username)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM login_tokens
            WHERE username = @username
              AND expires_at <= NOW()
        ";

        command.Parameters.AddWithValue("username", username);

        command.ExecuteNonQuery();
    }
}