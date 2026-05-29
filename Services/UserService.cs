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
        return _userRepository.FindAll();
    }

    public UserResponse? GetById(int id)
    {
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

        var passwordHash = _passwordHasher.HashPassword(dummyUser, request.Password);

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

        if (!string.IsNullOrWhiteSpace(request.Password) &&
            !IsValidPasswordLength(request.Password))
        {
            return new MessageResponse(false, "パスワードは8文字以上で入力してください。");
        }

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
            _userRepository.Update(id, request.Username, request.Role);
        }
        else
        {
            var dummyUser = new UserAuthInfo(
                id,
                request.Username,
                "",
                request.Role
            );

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

        _userRepository.DeleteWithLoginTokens(id);

        return new MessageResponse(true, "ユーザーを削除しました。");
    }

    private static bool IsValidPasswordLength(string password)
    {
        return password.Length >= MinimumPasswordLength;
    }

    private static bool IsValidRole(string role)
    {
        return role == "general" || role == "admin";
    }
}