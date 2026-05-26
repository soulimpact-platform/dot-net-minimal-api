// 認証処理を定義するインターフェース
public interface IAuthService
{
    // ユーザー名・パスワードを確認し、成功時はJWTを返す
    LoginResponse? Login(LoginRequest request);

    // 認証済みユーザー自身のJWTを削除してログアウト
    MessageResponse Logout(int userId, string token);
}