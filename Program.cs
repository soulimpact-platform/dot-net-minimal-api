using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQLへの接続文字列を設定
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=login_sample;Username=app_user;Password=app_password";

// JWTの発行・検証に使用する設定
var jwtSecret = "this_is_sample_jwt_secret_key_1234567890";
var jwtIssuer = "dot-net-minimal-api";
var jwtAudience = "dot-net-minimal-api-user";

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// アプリ起動時にDBとテーブルを作成し、初期データを登録
InitializeDatabase(connectionString);

// ログインAPI
// 画面から送信されたユーザー名・パスワードをDBのusersテーブルと照合し、成功時にJWTを発行する
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

    if (username is null)
    {
        return Results.Unauthorized();
    }

    // JWTの有効期限を設定
    var expiresAt = DateTime.UtcNow.AddHours(1);

    // ログインユーザー用のJWTを生成
    var token = CreateJwtToken(
        username,
        jwtSecret,
        jwtIssuer,
        jwtAudience,
        expiresAt
    );

    // 発行したJWTをDBに保存
    SaveLoginToken(connectionString, username, token, expiresAt);

    return Results.Ok(new
    {
        success = true,
        token = token,
        message = "ログイン成功"
    });
});

// 認証チェックAPI
// 各画面表示時にJWTを検証し、ログイン済みかどうかをサーバー側で判定する
app.MapPost("/api/auth/check", (TokenRequest request) =>
{
    // JWTの署名、有効期限、発行者、利用者を検証
    var username = ValidateJwtToken(
        request.Token,
        jwtSecret,
        jwtIssuer,
        jwtAudience
    );

    if (username is null)
    {
        return Results.Unauthorized();
    }

    // JWTがDBに保存されているか確認
    if (!ExistsLoginToken(connectionString, username, request.Token))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        success = true,
        username = username
    });
});

// ログアウトAPI
// DBに保存しているJWTを削除し、以降そのトークンを無効にする
app.MapPost("/api/logout", (TokenRequest request) =>
{
    DeleteLoginToken(connectionString, request.Token);

    return Results.Ok(new
    {
        success = true,
        message = "ログアウトしました"
    });
});

// 書籍検索API
// 書籍名のみ、カテゴリのみ、または両方指定で検索する
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

    if (!reader.Read())
    {
        return Results.NotFound();
    }

    var product = new ProductDetailResponse(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetInt32(3),
        reader.GetString(4)
    );

    return Results.Ok(product);
});

app.Run();

// DBとテーブルを初期作成し、初期データを登録
static void InitializeDatabase(string connectionString)
{
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

    // login_tokensテーブルを作成
    // 発行済みJWTを保存し、チェックAPIやログアウトAPIで利用
    var createLoginTokensTableCommand = connection.CreateCommand();
    createLoginTokensTableCommand.CommandText = @"
        CREATE TABLE IF NOT EXISTS login_tokens (
            id SERIAL PRIMARY KEY,
            username TEXT NOT NULL,
            token TEXT NOT NULL UNIQUE,
            expires_at TIMESTAMPTZ NOT NULL
        );
    ";
    createLoginTokensTableCommand.ExecuteNonQuery();

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

// ログイン成功時にJWTを生成
static string CreateJwtToken(
    string username,
    string jwtSecret,
    string jwtIssuer,
    string jwtAudience,
    DateTime expiresAt)
{
    // JWTに含めるユーザー情報を設定
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username)
    };

    // 秘密鍵を使って署名情報を作成
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // JWT本体を作成
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: expiresAt,
        signingCredentials: credentials
    );

    // JWTを文字列として返却
    return new JwtSecurityTokenHandler().WriteToken(token);
}

// 発行したJWTをDBに保存する
static void SaveLoginToken(
    string connectionString,
    string username,
    string token,
    DateTime expiresAt)
{
    using var connection = new NpgsqlConnection(connectionString);
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

// JWTの署名、有効期限、発行者、利用者を検証
static string? ValidateJwtToken(
    string token,
    string jwtSecret,
    string jwtIssuer,
    string jwtAudience)
{
    try
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        // JWT検証時の条件を設定
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // JWTを検証し、問題なければユーザー情報を取得
        var principal = tokenHandler.ValidateToken(token, parameters, out _);

        return principal.Identity?.Name;
    }
    catch
    {
        // 署名不正、期限切れ、形式不正などの場合は認証失敗とする
        return null;
    }
}

// JWTがDBに保存されていて有効期限内か確認
static bool ExistsLoginToken(
    string connectionString,
    string username,
    string token)
{
    using var connection = new NpgsqlConnection(connectionString);
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

// ログアウト時にDBからJWTを削除
static void DeleteLoginToken(string connectionString, string token)
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        DELETE FROM login_tokens
        WHERE token = @token
    ";

    command.Parameters.AddWithValue("token", token);

    command.ExecuteNonQuery();
}

// ログインAPIで受け取るリクエスト
record LoginRequest(string Username, string Password);

// JWTチェックAPI・ログアウトAPIで受け取るリクエスト
record TokenRequest(string Token);

// 書籍検索結果で返すレスポンス
record ProductResponse(int Id, string Name, string Category, int Price);

// 書籍詳細で返すレスポンス
record ProductDetailResponse(int Id, string Name, string Category, int Price, string Description);