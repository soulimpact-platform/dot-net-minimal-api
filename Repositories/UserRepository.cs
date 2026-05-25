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

    public UserAuthInfo? FindByUsername(string username)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // ユーザー名に一致する認証用ユーザー情報を取得
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                id AS id,
                username AS username,
                password_hash AS password_hash,
                role AS role
            FROM users
            WHERE username = @username
        ";

        command.Parameters.AddWithValue("username", username);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        // DBから取得した行を認証用ユーザー情報に変換
        return new UserAuthInfo(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password_hash")),
            reader.GetString(reader.GetOrdinal("role"))
        );
    }

    public void UpdatePasswordHash(int userId, string passwordHash)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // 指定ユーザーのパスワードハッシュを更新
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE users
            SET password_hash = @passwordHash
            WHERE id = @userId
        ";

        command.Parameters.AddWithValue("passwordHash", passwordHash);
        command.Parameters.AddWithValue("userId", userId);

        command.ExecuteNonQuery();
    }
}