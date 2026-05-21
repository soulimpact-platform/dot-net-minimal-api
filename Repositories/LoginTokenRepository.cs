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

    public void Save(int userId, string token, DateTime expiresAt)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO login_tokens (user_id, token, expires_at)
            VALUES (@userId, @token, @expiresAt);
        ";

        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("expiresAt", expiresAt);

        command.ExecuteNonQuery();
    }

    public bool Exists(int userId, string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM login_tokens
            WHERE user_id = @userId
              AND token = @token
              AND expires_at > NOW()
        ";

        command.Parameters.AddWithValue("userId", userId);
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

    public void DeleteExpired(int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM login_tokens
            WHERE user_id = @userId
              AND expires_at <= NOW()
        ";

        command.Parameters.AddWithValue("userId", userId);

        command.ExecuteNonQuery();
    }
}