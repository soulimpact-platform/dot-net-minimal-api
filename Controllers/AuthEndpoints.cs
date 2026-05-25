// 認証関連APIのエンドポイント定義
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // ログインAPI
        // ユーザー名・パスワードを受け取り、認証成功時にJWTを返す
        app.MapPost("/api/login", (LoginRequest request, IAuthService authService) =>
        {
            var result = authService.Login(request);

            if (result is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result);
        });

        // ログアウトAPI
        // DBに保存されているJWTを削除
        app.MapPost("/api/logout", (TokenRequest request, IAuthService authService) =>
        {
            var result = authService.Logout(request);

            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}