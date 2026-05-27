// ユーザー一覧・詳細表示用のレスポンス
public record UserResponse(
    int Id,
    string Username,
    string Role
);