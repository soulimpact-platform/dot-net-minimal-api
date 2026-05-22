// ユーザー情報の取得処理を定義するインターフェース
public interface IUserRepository
{
    // ユーザー名に一致するログイン認証用ユーザー情報を取得
    UserAuthInfo? FindByUsername(string username);
}