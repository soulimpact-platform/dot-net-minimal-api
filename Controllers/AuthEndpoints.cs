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
        // 認証済みユーザー自身のJWTをDBから削除
        app.MapPost("/api/logout", (HttpContext context, IAuthService authService) =>
        {
            var userIdText = context.User.FindFirst("user_id")?.Value;

            if (!int.TryParse(userIdText, out var userId))
            {
                return Results.Unauthorized();
            }

            var authorizationHeader = context.Request.Headers.Authorization.ToString();

            var token = authorizationHeader.Replace(
                "Bearer ",
                "",
                StringComparison.OrdinalIgnoreCase
            );

            if (string.IsNullOrEmpty(token))
            {
                return Results.Unauthorized();
            }

            var result = authService.Logout(userId, token);

            return Results.Ok(result);
        })
        .RequireAuthorization();
    }
}