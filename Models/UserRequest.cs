// ユーザー追加・編集時のリクエスト
public record UserRequest(
    string Username,
    string? Password,
    string Role
);