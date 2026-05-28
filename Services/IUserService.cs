// ユーザー管理処理を定義するServiceインターフェース
public interface IUserService
{
    // ユーザー一覧を取得
    List<UserResponse> GetAll();

    // IDに一致するユーザー情報を取得
    UserResponse? GetById(int id);

    // ユーザーを追加
    MessageResponse Create(UserRequest request);

    // ユーザーを更新
    MessageResponse Update(int id, int currentUserId, UserRequest request);

    // ユーザーを削除
    MessageResponse Delete(int id, int currentUserId);
}