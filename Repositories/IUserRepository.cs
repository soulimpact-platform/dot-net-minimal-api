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

    // 指定ID以外でユーザー名が存在するか確認
    bool ExistsByUsernameExceptId(string username, int id);

    // ユーザーを追加
    void Create(string username, string passwordHash, string role);

    // ユーザー情報を更新
    void Update(int id, string username, string role);

    // パスワードを含めてユーザー情報を更新
    void UpdateWithPassword(int id, string username, string passwordHash, string role);

    // 指定ユーザーのパスワードハッシュを更新
    void UpdatePasswordHash(int userId, string passwordHash);

    // ユーザーを削除
    void Delete(int id);
}