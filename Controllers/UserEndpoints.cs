// ユーザー管理APIを登録するエンドポイント
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        // ユーザー一覧取得API
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
        app.MapPost("/api/admin/users", (UserRequest request, IUserService userService) =>
        {
            var result = userService.Create(request);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー更新API
        app.MapPut("/api/admin/users/{id:int}", (int id, UserRequest request, IUserService userService) =>
        {
            var result = userService.Update(id, request);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // ユーザー削除API
        app.MapDelete("/api/admin/users/{id:int}", (int id, IUserService userService) =>
        {
            var result = userService.Delete(id);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));
    }
}