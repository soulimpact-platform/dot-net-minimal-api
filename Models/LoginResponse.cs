// ログイン成功時に返すレスポンス
public record LoginResponse(
    bool Success,
    string Token,
    string Message
);