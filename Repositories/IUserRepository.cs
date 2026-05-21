// ユーザー情報の取得処理を定義するインターフェース
public interface IUserRepository
{
    // ユーザー名とパスワードに一致するユーザー情報を取得
    AuthenticatedUser? FindByUsernameAndPassword(string username, string password);
}