using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

// ログイン、JWT発行、JWT検証を行うService
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

        // JWTの発行者と利用者を設定
        _jwtIssuer = "dot-net-minimal-api";
        _jwtAudience = "dot-net-minimal-api-user";
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

    public AuthCheckResponse? Check(TokenRequest request)
    {
        // JWTの署名、有効期限、発行者、利用者を検証
        var user = ValidateJwtToken(request.Token);

        if (user is null)
        {
            return null;
        }

        // JWTがDBに保存されていて有効期限内か確認
        if (!_loginTokenRepository.Exists(user.Id, request.Token))
        {
            return null;
        }

        return new AuthCheckResponse(
            true,
            user.Username,
            user.Role
        );
    }

    public MessageResponse Logout(TokenRequest request)
    {
        // DBに保存しているJWTを削除
        _loginTokenRepository.Delete(request.Token);

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
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
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

    private AuthenticatedUser? ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            // JWT検証時の条件を設定
            var parameters = new TokenValidationParameters
            {
                // 署名に使用した秘密鍵が正しいか検証
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                // 発行者が想定通りか検証
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,

                // 利用者が想定通りか検証
                ValidateAudience = true,
                ValidAudience = _jwtAudience,

                // 有効期限が切れていないか検証
                ValidateLifetime = true,

                // 有効期限のずれを許容しない
                ClockSkew = TimeSpan.Zero
            };

            // JWTを検証し、問題なければユーザー情報を取得
            var principal = tokenHandler.ValidateToken(token, parameters, out _);

            var userIdText = principal.FindFirst("user_id")?.Value;
            var username = principal.Identity?.Name;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdText, out var userId) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(role))
            {
                return null;
            }

            return new AuthenticatedUser(
                userId,
                username,
                role
            );
        }
        catch
        {
            // 署名不正、期限切れ、形式不正などの場合は認証失敗とする
            return null;
        }
    }
}