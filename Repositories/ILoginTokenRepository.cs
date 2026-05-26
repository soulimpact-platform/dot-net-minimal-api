// JWTの保存・確認・削除処理を定義するインターフェース
public interface ILoginTokenRepository
{
    // 発行したJWTをDBに保存
    void Save(int userId, string token, DateTime expiresAt);

    // JWTがDBに保存されていて有効期限内か非同期で確認
    Task<bool> ExistsAsync(int userId, string token);

    // 指定ユーザーのJWTのみ削除
    void DeleteByUserAndToken(int userId, string token);

    // 指定ユーザーの期限切れJWTを削除
    void DeleteExpired(int userId);
}