// ユーザー情報の取得・更新処理を定義するインターフェース
public interface IUserRepository
{
    // ユーザー名に一致するログイン認証用ユーザー情報を取得
    UserAuthInfo? FindByUsername(string username);

    // 指定ユーザーのパスワードハッシュを更新
    void UpdatePasswordHash(int userId, string passwordHash);
}