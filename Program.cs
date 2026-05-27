using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// JWTの署名に使用するシークレットキーを取得
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured");

// JWTの発行者を取得
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured");

// JWTの利用者を取得
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured");

// 使用するRepositoryを登録
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginTokenRepository, LoginTokenRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 使用するServiceを登録
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();

// パスワードハッシュ検証用の処理を登録
builder.Services.AddScoped<IPasswordHasher<UserAuthInfo>, PasswordHasher<UserAuthInfo>>();

// JWT Bearer認証を設定
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // JWTのクレーム名を自動変換しない
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 署名に使用した秘密鍵が正しいか検証
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

            // 発行者が想定通りか検証
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // 利用者が想定通りか検証
            ValidateAudience = true,
            ValidAudience = jwtAudience,

            // 有効期限が切れていないか検証
            ValidateLifetime = true,

            // 有効期限のずれを許容しない
            ClockSkew = TimeSpan.Zero,

            // JWT内の短いクレーム名をユーザー名・ロールとして扱う
            NameClaimType = "username",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                // 標準のJWT検証はここまでで通過済み。続けてDB照合
                var repo = ctx.HttpContext.RequestServices.GetRequiredService<ILoginTokenRepository>();

                var userIdText = ctx.Principal?.FindFirst("user_id")?.Value;

                if (!int.TryParse(userIdText, out var userId))
                {
                    ctx.Fail("Invalid user_id");
                    return;
                }

                var token = (ctx.SecurityToken as JsonWebToken)?.EncodedToken ?? "";

                if (string.IsNullOrEmpty(token) || !await repo.ExistsAsync(userId, token))
                {
                    ctx.Fail("Token revoked");
                }
            }
        };
    });

// 認可を設定
builder.Services.AddAuthorization();

var app = builder.Build();

// wwwroot配下のファイルを表示できるよう設定
app.UseDefaultFiles();
app.UseStaticFiles();

// 認証・認可を有効化
app.UseAuthentication();
app.UseAuthorization();

// APIエンドポイントを登録
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapUserEndpoints();

app.Run();