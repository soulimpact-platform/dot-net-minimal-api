// 認証処理を定義するインターフェース
public interface IAuthService
{
    // ユーザー名・パスワードを確認し、成功時はJWTを返す
    LoginResponse? Login(LoginRequest request);

    // JWTを検証し、ログイン状態を確認
    AuthCheckResponse? Check(TokenRequest request);

    // JWTを削除してログアウト
    MessageResponse Logout(TokenRequest request);
}