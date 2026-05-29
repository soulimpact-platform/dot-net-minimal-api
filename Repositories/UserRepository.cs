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

        return new UserAuthInfo(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password_hash")),
            reader.GetString(reader.GetOrdinal("role"))
        );
    }

    public List<UserResponse> FindAll()
    {
        var users = new List<UserResponse>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                id AS id,
                username AS username,
                role AS role
            FROM users
            ORDER BY id ASC
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            users.Add(CreateUserResponse(reader));
        }

        return users;
    }

    public UserResponse? FindById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                id AS id,
                username AS username,
                role AS role
            FROM users
            WHERE id = @id
        ";

        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return CreateUserResponse(reader);
    }

    public bool ExistsByUsername(string username)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS(
                SELECT 1
                FROM users
                WHERE username = @username
            )
        ";

        command.Parameters.AddWithValue("username", username);

        return (bool)command.ExecuteScalar()!;
    }

    public bool ExistsByUsernameExceptId(string username, int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS(
                SELECT 1
                FROM users
                WHERE username = @username
                  AND id <> @id
            )
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("id", id);

        return (bool)command.ExecuteScalar()!;
    }

    public void Create(string username, string passwordHash, string role)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO users (username, password_hash, role)
            VALUES (@username, @passwordHash, @role)
        ";

        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("passwordHash", passwordHash);
        command.Parameters.AddWithValue("role", role);

        command.ExecuteNonQuery();
    }

    public void Update(int id, string username, string role)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE users
            SET username = @username,
                role = @role
            WHERE id = @id
        ";

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("role", role);

        command.ExecuteNonQuery();
    }

    public void UpdateWithPassword(int id, string username, string passwordHash, string role)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE users
            SET username = @username,
                password_hash = @passwordHash,
                role = @role
            WHERE id = @id
        ";

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("passwordHash", passwordHash);
        command.Parameters.AddWithValue("role", role);

        command.ExecuteNonQuery();
    }

    public void UpdatePasswordHash(int userId, string passwordHash)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

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

    public bool HasLoanHistory(int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS(
                SELECT 1
                FROM book_loans
                WHERE user_id = @userId
            )
        ";

        command.Parameters.AddWithValue("userId", userId);

        return (bool)command.ExecuteScalar()!;
    }

    public void DeleteWithLoginTokens(int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var deleteTokensCommand = connection.CreateCommand();
        deleteTokensCommand.Transaction = transaction;
        deleteTokensCommand.CommandText = @"
            DELETE FROM login_tokens
            WHERE user_id = @userId
        ";

        deleteTokensCommand.Parameters.AddWithValue("userId", userId);
        deleteTokensCommand.ExecuteNonQuery();

        var deleteUserCommand = connection.CreateCommand();
        deleteUserCommand.Transaction = transaction;
        deleteUserCommand.CommandText = @"
            DELETE FROM users
            WHERE id = @userId
        ";

        deleteUserCommand.Parameters.AddWithValue("userId", userId);
        deleteUserCommand.ExecuteNonQuery();

        transaction.Commit();
    }

    private static UserResponse CreateUserResponse(NpgsqlDataReader reader)
    {
        return new UserResponse(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("role"))
        );
    }
}