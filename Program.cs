var builder = WebApplication.CreateBuilder(args);

// 使用するRepositoryを登録
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginTokenRepository, LoginTokenRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 使用するServiceを登録
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// wwwroot配下のファイルを表示できるよう設定
app.UseDefaultFiles();
app.UseStaticFiles();

// APIエンドポイントを登録
app.MapAuthEndpoints();
app.MapProductEndpoints();

app.Run();