// JWTの保存・確認・削除処理を定義するインターフェース
public interface ILoginTokenRepository
{
    // 発行したJWTをDBに保存
    void Save(int userId, string token, DateTime expiresAt);

    // JWTがDBに保存されていて有効期限内か確認
    bool Exists(int userId, string token);

    // ログアウト時にJWTを削除
    void Delete(string token);

    // 指定ユーザーの期限切れJWTを削除
    void DeleteExpired(int userId);
}