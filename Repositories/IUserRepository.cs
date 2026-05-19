// ユーザー情報の取得処理を定義するインターフェース
public interface IUserRepository
{
    // ユーザー名とパスワードに一致するユーザー名を取得
    string? FindUsername(string username, string password);
}