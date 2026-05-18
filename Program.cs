using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// PostgreSQLへの接続文字列を設定
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=login_sample;Username=app_user;Password=app_password";

// アプリ起動時にDBとテーブルを作成し、初期データを登録
InitializeDatabase(connectionString);

// ログインAPI
// 画面から送信されたユーザー名・パスワードをDBのusersテーブルと照合
app.MapPost("/api/login", (LoginRequest request) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT username
        FROM users
        WHERE username = @username
          AND password = @password
    ";

    command.Parameters.AddWithValue("username", request.Username);
    command.Parameters.AddWithValue("password", request.Password);

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

// 書籍検索API
// 書籍名とカテゴリをAND条件で検索
app.MapGet("/api/products/search", (string name, string category) =>
{
    var products = new List<ProductResponse>();

    using var connection = new NpgsqlConnection(connectionString);
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

    return Results.Ok(products);
});

// 書籍詳細API
// 指定されたIDの書籍詳細を取得
app.MapGet("/api/products/{id:int}", (int id) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT id, name, category, price, description
      FROM products
       WHERE id = @id
    ";

    command.Parameters.AddWithValue("id", id);

    using var reader = command.ExecuteReader();

    if (reader.Read())
    {
        var product = new ProductDetailResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4)
        );

        return Results.Ok(product);
    }

    return Results.NotFound();
});

app.Run();

static void InitializeDatabase(string connectionString)
{
    var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

    // DBファイルを配置するDataフォルダが存在しない場合は作成
    if (!Directory.Exists(dataDirectory))
    {
        Directory.CreateDirectory(dataDirectory);
    }

    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    // usersテーブルを作成
    var createUsersTableCommand = connection.CreateCommand();
    createUsersTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL
        );
    ";
    createUsersTableCommand.ExecuteNonQuery();

    // productsテーブルを作成
    var createProductsTableCommand = connection.CreateCommand();
    createProductsTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS products (
            id INTEGER PRIMARY KEY,
            name TEXT NOT NULL,
            category TEXT NOT NULL,
            price INTEGER NOT NULL,
           description TEXT NOT NULL
        );
    ";
    createProductsTableCommand.ExecuteNonQuery();

    // 初期ログインユーザーを登録
    var insertUsersCommand = connection.CreateCommand();
    insertUsersCommand.CommandText = @"
        INSERT INTO users (username, password)
        VALUES
            ('user01', 'password01'),
            ('user02', 'password02'),
            ('user03', 'password03'),
            ('user04', 'password04')
        ON CONFLICT (username) DO NOTHING;
    ";
    insertUsersCommand.ExecuteNonQuery();

    // 初期書籍データを登録
    var insertProductsCommand = connection.CreateCommand();
    insertProductsCommand.CommandText = @"
        INSERT INTO products (id, name, category, price, description)
        VALUES
            (1, '独習C#', '技術書', 3600, 'C#の基本を学べる入門書'),
            (2, 'なるほどなっとくC#入門', '技術書', 3000, 'C#の基礎を分かりやすく解説した書籍'),
            (3, '独習JavaScript 新版', '技術書', 3200, 'JavaScriptの基本を学べる入門書'),

            (4, '吾輩は猫である', '小説', 800, '夏目漱石による風刺小説'),
            (5, '坊っちゃん', '小説', 700, '夏目漱石による青春小説'),
            (6, '銀河鉄道の夜', '小説', 750, '宮沢賢治による幻想小説'),

            (7, '日本の歴史', '歴史', 1200, '日本史の流れを学べる書籍'),
            (8, '世界の歴史', '歴史', 1300, '世界史の流れを学べる書籍'),

            (9, '英単語ターゲット1900', '語学', 1100, '英単語を学習するための単語集'),
            (10, '速読英単語 必修編', '語学', 1200, '英文を読みながら単語を学べる書籍')
        ON CONFLICT (id) DO NOTHING;
    ";
    insertProductsCommand.ExecuteNonQuery();
}

record LoginRequest(string Username, string Password);
record ProductResponse(int Id, string Name, string Category, int Price);
record ProductDetailResponse(int Id, string Name, string Category, int Price, string Description);