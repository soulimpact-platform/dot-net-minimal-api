using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "app.db");
var connectionString = $"Data Source={dbPath}";

// DBとテーブルを初期作成する
InitializeDatabase(connectionString);

app.MapGet("/api/hello", () => "Hello World!");

app.MapPost("/api/login", (LoginRequest request) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT username
        FROM users
        WHERE username = $username
          AND password = $password
    ";

    command.Parameters.AddWithValue("$username", request.Username);
    command.Parameters.AddWithValue("$password", request.Password);

    var username = command.ExecuteScalar() as string;

    if (username is not null)
    {
        return Results.Ok(new
        {
            success = true,
            username = username,
            message = "ログイン成功"
        });
    }

    return Results.Unauthorized();
});

app.Run();

static void InitializeDatabase(string connectionString)
{
    var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

    if (!Directory.Exists(dataDirectory))
    {
        Directory.CreateDirectory(dataDirectory);
    }

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var createTableCommand = connection.CreateCommand();
    createTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL
        );
    ";
    createTableCommand.ExecuteNonQuery();

var insertCommand = connection.CreateCommand();
insertCommand.CommandText = @"
    INSERT OR IGNORE INTO users (username, password)
    VALUES
        ('user01', 'password01'),
        ('user02', 'password02'),
        ('user03', 'password03'),
        ('user04', 'password04');
";
insertCommand.ExecuteNonQuery();
}

record LoginRequest(string Username, string Password);