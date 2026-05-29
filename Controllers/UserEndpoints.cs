// ユーザー管理APIを登録するエンドポイント
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // ユーザー一覧取得API
        // TODO: ユーザー件数が増えた場合はページング対応を検討する
        app.MapGet("/api/admin/users", (IUserService userService) =>
        {
            var users = userService.GetAll();

            return Results.Ok(users);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー詳細取得API
        app.MapGet("/api/admin/users/{id:int}", (int id, IUserService userService) =>
        {
            var user = userService.GetById(id);

            if (user is null)
            {
                return Results.NotFound(new MessageResponse(false, "ユーザーが見つかりません。"));
            }

            return Results.Ok(user);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー追加API
        app.MapPost("/api/admin/users", (UserRequest request, HttpContext context, IUserService userService) =>
        {
            var currentUsername = GetUsername(context);

            if (string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = userService.Create(request, currentUsername);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー更新API
        app.MapPut("/api/admin/users/{id:int}", (int id, UserRequest request, HttpContext context, IUserService userService) =>
        {
            var currentUserId = GetUserId(context);
            var currentUsername = GetUsername(context);

            if (currentUserId is null || string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = userService.Update(id, currentUserId.Value, currentUsername, request);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー削除API
        app.MapDelete("/api/admin/users/{id:int}", (int id, HttpContext context, IUserService userService) =>
        {
            var currentUserId = GetUserId(context);
            var currentUsername = GetUsername(context);

            if (currentUserId is null || string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = userService.Delete(id, currentUserId.Value, currentUsername);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));
    }

    private static int? GetUserId(HttpContext context)
    {
        var userIdText = context.User.FindFirst("user_id")?.Value;

        if (!int.TryParse(userIdText, out var userId))
        {
            return null;
        }

        return userId;
    }

    private static string? GetUsername(HttpContext context)
    {
        return context.User.FindFirst("username")?.Value;
    }
}
