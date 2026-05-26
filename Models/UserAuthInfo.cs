// ログイン認証で使用するユーザー情報
public record UserAuthInfo(
    int Id,
    string Username,
    string PasswordHash,
    string Role
);