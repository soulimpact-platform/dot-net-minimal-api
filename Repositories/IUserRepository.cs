// ユーザー情報の取得・更新処理を定義するインターフェース
public interface IUserRepository
{
    // ユーザー名に一致するログイン認証用ユーザー情報を取得
    UserAuthInfo? FindByUsername(string username);

    // ユーザー一覧を取得
    List<UserResponse> FindAll();

    // IDに一致するユーザー情報を取得
    UserResponse? FindById(int id);

    // ユーザー名が存在するか確認
    bool ExistsByUsername(string username);

    // ユーザーを追加
    void Create(string username, string passwordHash, string role, string createdBy);

    // ユーザー情報を更新
    void Update(int id, string role, string updatedBy);

    // パスワードを含めてユーザー情報を更新
    void UpdateWithPassword(int id, string passwordHash, string role, string updatedBy);

    // 指定ユーザーのパスワードハッシュを更新
    void UpdatePasswordHash(int userId, string passwordHash);

    // ログイントークン削除とユーザー論理削除を同一トランザクションで実行
    void DeleteWithLoginTokens(int userId, string updatedBy);
}
