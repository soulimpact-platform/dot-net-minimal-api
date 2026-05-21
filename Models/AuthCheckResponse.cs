// 認証チェック成功時に返すレスポンス
public record AuthCheckResponse(
    bool Success,
    string Username,
    string Role
);