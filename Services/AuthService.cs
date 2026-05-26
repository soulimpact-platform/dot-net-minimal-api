using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

// ログイン、JWT発行、ログアウトを行うService
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILoginTokenRepository _loginTokenRepository;
    private readonly IPasswordHasher<UserAuthInfo> _passwordHasher;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(
        IUserRepository userRepository,
        ILoginTokenRepository loginTokenRepository,
        IConfiguration configuration,
        IPasswordHasher<UserAuthInfo> passwordHasher)
    {
        _userRepository = userRepository;
        _loginTokenRepository = loginTokenRepository;
        _passwordHasher = passwordHasher;

        // JWTの署名に使用するシークレットキーを取得
        _jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");

        // JWTの発行者を取得
        _jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");

        // JWTの利用者を取得
        _jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");
    }

    public LoginResponse? Login(LoginRequest request)
    {
        // ユーザー名に一致するユーザー情報を取得
        var user = _userRepository.FindByUsername(request.Username);

        if (user is null)
        {
            return null;
        }

        // 入力されたパスワードとDBに保存されているハッシュ値を検証
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // ハッシュ再計算が推奨される場合は、新しいハッシュ値でDBを更新
            var newPasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _userRepository.UpdatePasswordHash(user.Id, newPasswordHash);
        }

        var authenticatedUser = new AuthenticatedUser(
            user.Id,
            user.Username,
            user.Role
        );

        // JWTの有効期限を設定
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // ログインユーザー用のJWTを生成
        var token = CreateJwtToken(authenticatedUser, expiresAt);

        // 同一ユーザーの期限切れJWTを削除
        _loginTokenRepository.DeleteExpired(authenticatedUser.Id);

        // 発行したJWTをDBに保存
        _loginTokenRepository.Save(authenticatedUser.Id, token, expiresAt);

        return new LoginResponse(
            true,
            token,
            "ログイン成功"
        );
    }

    public MessageResponse Logout(int userId, string token)
    {
        // 認証済みユーザー自身のJWTのみ削除
        _loginTokenRepository.DeleteByUserAndToken(userId, token);

        return new MessageResponse(
            true,
            "ログアウトしました"
        );
    }

    private string CreateJwtToken(AuthenticatedUser user, DateTime expiresAt)
    {
        // JWTに含めるユーザー情報を設定
        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim("role", user.Role)
        };

        // 秘密鍵を使って署名情報を作成
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // JWTを作成
        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        // JWTを文字列として返却
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}