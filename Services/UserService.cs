using Microsoft.AspNetCore.Identity;

// ユーザー管理処理を行うService
public class UserService : IUserService
{
    private const int MinimumPasswordLength = 8;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<UserAuthInfo> _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<UserAuthInfo> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public List<UserResponse> GetAll()
    {
        // ユーザー一覧を取得
        return _userRepository.FindAll();
    }

    public UserResponse? GetById(int id)
    {
        // IDに一致するユーザー情報を取得
        return _userRepository.FindById(id);
    }

    public MessageResponse Create(UserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new MessageResponse(false, "ユーザー名を入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new MessageResponse(false, "パスワードを入力してください。");
        }

        if (!IsValidPasswordLength(request.Password))
        {
            return new MessageResponse(false, "パスワードは8文字以上で入力してください。");
        }

        if (!IsValidRole(request.Role))
        {
            return new MessageResponse(false, "ロールが不正です。");
        }

        if (_userRepository.ExistsByUsername(request.Username))
        {
            return new MessageResponse(false, "同じユーザー名が既に存在します。");
        }

        var dummyUser = new UserAuthInfo(
            0,
            request.Username,
            "",
            request.Role
        );

        // パスワードをハッシュ化
        var passwordHash = _passwordHasher.HashPassword(dummyUser, request.Password);

        // ユーザーを追加
        _userRepository.Create(request.Username, passwordHash, request.Role);

        return new MessageResponse(true, "ユーザーを追加しました。");
    }

    public MessageResponse Update(int id, int currentUserId, UserRequest request)
    {
        var user = _userRepository.FindById(id);

        if (user is null)
        {
            return new MessageResponse(false, "ユーザーが見つかりません。");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new MessageResponse(false, "ユーザー名を入力してください。");
        }

        // 更新時のパスワードは任意。未入力の場合は変更しない
        if (!IsValidRole(request.Role))
        {
            return new MessageResponse(false, "ロールが不正です。");
        }

        if (id == currentUserId && request.Role == "general")
        {
            return new MessageResponse(false, "ログイン中のユーザーは一般ユーザーに変更できません。");
        }

        if (_userRepository.ExistsByUsernameExceptId(request.Username, id))
        {
            return new MessageResponse(false, "同じユーザー名が既に存在します。");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            // パスワード未入力の場合は、ユーザー名とロールのみ更新
            _userRepository.Update(id, request.Username, request.Role);
        }
        else
        {
            if (!IsValidPasswordLength(request.Password))
            {
                return new MessageResponse(false, "パスワードは8文字以上で入力してください。");
            }

            var dummyUser = new UserAuthInfo(
                id,
                request.Username,
                "",
                request.Role
            );

            // 入力されたパスワードをハッシュ化して更新
            var passwordHash = _passwordHasher.HashPassword(dummyUser, request.Password);

            _userRepository.UpdateWithPassword(
                id,
                request.Username,
                passwordHash,
                request.Role
            );
        }

        return new MessageResponse(true, "ユーザーを更新しました。");
    }

    public MessageResponse Delete(int id, int currentUserId)
    {
        var user = _userRepository.FindById(id);

        if (user is null)
        {
            return new MessageResponse(false, "ユーザーが見つかりません。");
        }

        if (id == currentUserId)
        {
            return new MessageResponse(false, "ログイン中のユーザーは削除できません。");
        }

        if (_userRepository.HasLoanHistory(id))
        {
            return new MessageResponse(false, "貸出履歴があるユーザーは削除できません。");
        }

        // login_tokens の外部キー制約に引っかからないよう、先にログイントークンを削除
        _userRepository.DeleteLoginTokens(id);

        // ユーザーを削除
        _userRepository.Delete(id);

        return new MessageResponse(true, "ユーザーを削除しました。");
    }

    // パスワードの最低文字数を満たしているか確認
    private static bool IsValidPasswordLength(string password)
    {
        return password.Length >= MinimumPasswordLength;
    }

    // 許可されたロールか確認
    private static bool IsValidRole(string role)
    {
        return role == "general" || role == "admin";
    }
}