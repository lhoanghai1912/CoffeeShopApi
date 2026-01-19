using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace CoffeeShopApi.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Đăng ký tài khoản mới - gửi mã xác thực qua email
    /// </summary>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Kiểm tra user có thể login không (active, credentials đúng)
    /// </summary>
    Task<(bool CanLogin, string? ErrorMessage, User? User)> ValidateLoginAsync(string username, string password);
    
    
    
    /// <summary>
    /// Xác thực email với mã 6 số
    /// </summary>
    // Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);
    
    /// <summary>
    /// Gửi lại mã xác thực email
    /// </summary>
    // Task<ResendVerificationResponse> ResendVerificationCodeAsync(ResendVerificationRequest request);
    
    /// <summary>
    /// Yêu cầu reset password - tạo mã và gửi email
    /// </summary>
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    
    /// <summary>
    /// Reset password với mã
    /// </summary>
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
    
    /// <summary>
    /// Kiểm tra reset token có hợp lệ không
    /// </summary>
    Task<bool> ValidateResetTokenAsync(string emailOrUsername, string token);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IRoleService _roleService;
    private readonly IEmailService _emailService;

    public AuthService(
        AppDbContext context, 
        IConfiguration configuration, 
        IRoleService roleService,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _roleService = roleService;
        _emailService = emailService;
    }

    #region Validate
    
    /// <summary>
    /// Validate login credentials và trạng thái user
    /// </summary>
    public async Task<(bool CanLogin, string? ErrorMessage, User? User)> ValidateLoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return (false, "Sai tài khoản hoặc mật khẩu", null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            return (false, "Sai tài khoản hoặc mật khẩu", null);

        // ✅ Kiểm tra email đã verify chưa
        if (!user.IsEmailVerified)
            return (false, "Vui lòng xác thực email trước khi đăng nhập", null);

        // ✅ Kiểm tra user có active không
        if (!user.IsActive)
            return (false, "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ.", null);

        return (true, null, user);
    }

    #endregion

    #region  Login Method
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var (canLogin, errorMessage, user) = await ValidateLoginAsync(request.Username, request.Password);
        
        if (!canLogin || user == null)
        {
            return null;
        }

        // ✅ Cập nhật LastLoginAt
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        RoleResponse? roleResponse = _roleService.ToRoleResponse(user.Role);

        // Lấy danh sách permissions của user
        var permissions = user.Role?.RolePermissions?
            .Select(rp => rp.Permission.Code)
            .ToList() ?? new List<string>();

        var token = GenerateJwtToken(user, permissions);

        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = roleResponse,
            PhoneNumber = user.PhoneNumber,
            Token = token 
        };
    }

    

    #endregion

    #region JwtToken

    // Hàm phụ trợ để sinh chuỗi JWT với permissions
    private string GenerateJwtToken(User user, List<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new("username", user.Username),
            new("fullName", user.FullName ?? ""),
            new("isActive", user.IsActive.ToString())
        };

        // Thêm role claims nếu có
        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Code));
            claims.Add(new Claim("roleId", user.Role.Id.ToString()));
            claims.Add(new Claim("roleName", user.Role.Name));
        }

        // Thêm permissions vào claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8), // Token sống 8 giờ
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    

    #endregion

    #region Register
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // ✅ Kiểm tra username đã tồn tại
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return new RegisterResponse
            {
                UserId = 0,
                Email = request.Email,
                Message = "Tên đăng nhập đã tồn tại",
                RequiresEmailVerification = false
            };
        }

        // ✅ Kiểm tra email đã tồn tại
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new RegisterResponse
            {
                UserId = 0,
                Email = request.Email,
                Message = "Email đã được sử dụng",
                RequiresEmailVerification = false
            };
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // ✅ BẢO MẬT: Không cho user tự chọn role admin (roleId = 1)
        // Mặc định tạo account customer (roleId = 2)
        int roleId = 2; // CUSTOMER
        
        // Chỉ cho phép admin tạo account với role khác
        // (Logic này sẽ được implement sau khi có authorization)
        if (request.RoleId.HasValue && request.RoleId.Value != 1)
        {
            // Validate roleId tồn tại
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId.Value);
            if (roleExists)
            {
                roleId = request.RoleId.Value;
            }
        }

        // Tạo mã xác thực email (6 số)
        var verificationCode = Generate6DigitCode();
        var hashedCode = HashToken(verificationCode);

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = passwordHash,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            RoleId = roleId,
            
            IsActive = true, // Chưa active cho đến khi verify email
            IsEmailVerified = true,
            EmailVerificationCode = hashedCode,
            EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(EMAIL_VERIFICATION_EXPIRY_MINUTES),
            EmailVerificationRequestCount = 1,
            LastEmailVerificationRequest = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };      

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Gửi email chào mừng
        await _emailService.SendWelcomeEmailAsync(request.Email, request.FullName);
        
        // // Gửi email xác thực
        // await _emailService.SendEmailVerificationCodeAsync(request.Email, request.FullName ?? request.Username, verificationCode);

        // Tạo token như khi đăng nhập để auto-login sau đăng ký
        var permissions = newUser.Role != null
            ? newUser.Role.RolePermissions?.Select(rp => rp.Permission.Code).ToList() ?? new List<string>()
            : new List<string>();

        var token = GenerateJwtToken(newUser, permissions);

        return new RegisterResponse
        {
            UserId = newUser.Id,
            Email = request.Email,
            Message = "Đăng ký thành công.",
            RequiresEmailVerification = false,
            VerificationCode = null,
            ExpiresInMinutes = RESET_TOKEN_EXPIRY_MINUTES,
            Token = token
        };
    }
    
    #endregion

    #region Email Verification Methods

    /// <summary>
    /// Cấu hình cho email verification
    /// </summary>
    private const int EMAIL_VERIFICATION_EXPIRY_MINUTES = 15;
    private const int MAX_EMAIL_VERIFICATION_REQUESTS_PER_DAY = 5;

    // public async Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
    // {
    //     var user = await _context.Users
    //         .Include(u => u.Role)
    //             .ThenInclude(r => r.RolePermissions)
    //             .ThenInclude(rp => rp.Permission)
    //         .FirstOrDefaultAsync(u => u.Email == request.Email);
    //
    //     if (user == null)
    //     {
    //         return new VerifyEmailResponse
    //         {
    //             Success = false,
    //             Message = "Email không tồn tại trong hệ thống."
    //         };
    //     }
    //
    //     if (user.IsEmailVerified)
    //     {
    //         return new VerifyEmailResponse
    //         {
    //             Success = false,
    //             Message = "Email đã được xác thực trước đó."
    //         };
    //     }
    //
    //     // Validate verification code
    //     if (!IsValidVerificationCode(user, request.VerificationCode))
    //     {
    //         return new VerifyEmailResponse
    //         {
    //             Success = false,
    //             Message = "Mã xác thực không đúng hoặc đã hết hạn."
    //         };
    //     }
    //
    //     // Xác thực thành công
    //     user.IsEmailVerified = true;
    //     user.EmailVerifiedAt = DateTime.UtcNow;
    //     user.IsActive = true; // Kích hoạt tài khoản
    //     user.EmailVerificationCode = null;
    //     user.EmailVerificationCodeExpiry = null;
    //     user.UpdatedAt = DateTime.UtcNow;
    //
    //     await _context.SaveChangesAsync();
    //
    //     // Tạo token để auto-login
    //     var permissions = user.Role?.RolePermissions?
    //         .Select(rp => rp.Permission.Code)
    //         .ToList() ?? new List<string>();
    //     var token = GenerateJwtToken(user, permissions);
    //
    //     return new VerifyEmailResponse
    //     {
    //         Success = true,
    //         Message = "Xác thực email thành công! Bạn có thể đăng nhập ngay.",
    //         Token = token
    //     };
    // }
    //
    // public async Task<ResendVerificationResponse> ResendVerificationCodeAsync(ResendVerificationRequest request)
    // {
    //     var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    //
    //     if (user == null)
    //     {
    //         // Không tiết lộ email có tồn tại hay không
    //         return new ResendVerificationResponse
    //         {
    //             Success = true,
    //             Message = "Nếu email tồn tại, mã xác thực mới sẽ được gửi.",
    //             ExpiresInMinutes = EMAIL_VERIFICATION_EXPIRY_MINUTES
    //         };
    //     }
    //
    //     if (user.IsEmailVerified)
    //     {
    //         return new ResendVerificationResponse
    //         {
    //             Success = false,
    //             Message = "Email đã được xác thực, không cần gửi lại mã."
    //         };
    //     }
    //
    //     // Kiểm tra rate limit
    //     if (!CanRequestEmailVerification(user))
    //     {
    //         return new ResendVerificationResponse
    //         {
    //             Success = false,
    //             Message = $"Bạn đã yêu cầu gửi mã quá {MAX_EMAIL_VERIFICATION_REQUESTS_PER_DAY} lần trong ngày. Vui lòng thử lại sau."
    //         };
    //     }
    //
    //     // Tạo mã mới
    //     var verificationCode = Generate6DigitCode();
    //     user.EmailVerificationCode = HashToken(verificationCode);
    //     user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(EMAIL_VERIFICATION_EXPIRY_MINUTES);
    //     user.EmailVerificationRequestCount = GetTodayEmailVerificationCount(user) + 1;
    //     user.LastEmailVerificationRequest = DateTime.UtcNow;
    //     user.UpdatedAt = DateTime.UtcNow;
    //
    //     await _context.SaveChangesAsync();
    //
    //     // Gửi email
    //     await _emailService.SendEmailVerificationCodeAsync(request.Email, user.FullName ?? user.Username, verificationCode);
    //
    //     var isDevelopment = _configuration.GetValue<bool>("IsDevelopment", false);
    //     var includeCodes = isDevelopment;
    //
    //     return new ResendVerificationResponse
    //     {
    //         Success = true,
    //         Message = "Mã xác thực mới đã được gửi đến email của bạn.",
    //         VerificationCode = includeCodes ? verificationCode : null,
    //         ExpiresInMinutes = EMAIL_VERIFICATION_EXPIRY_MINUTES
    //     };
    // }

    #endregion

    #region Forgot Password Methods

    /// <summary>
    /// Cấu hình cho forgot password
    /// </summary>
    private const int RESET_TOKEN_EXPIRY_MINUTES = 30;
    private const int MAX_RESET_REQUESTS_PER_DAY = 100;

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        // Tìm user theo email hoặc username
        var user = await FindUserByEmailOrUsernameAsync(request.EmailOrUsername);


        // Không tiết lộ user có tồn tại hay không (bảo mật)
        if (user == null)
        {
            return new ForgotPasswordResponse
            {
                Success = true, // Luôn trả success để không leak thông tin
                Message = "Nếu tài khoản tồn tại, bạn sẽ nhận được mã reset password qua email.",
                ExpiresInMinutes = RESET_TOKEN_EXPIRY_MINUTES
            };
        }

        // Kiểm tra email đã verify chưa
        if (!user.IsEmailVerified || string.IsNullOrEmpty(user.Email))
        {
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = "Tài khoản chưa xác thực email. Vui lòng xác thực email trước."
            };
        }

        // Kiểm tra user có active không
        if (!user.IsActive)
        {
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ."
            };
        }

        // Kiểm tra rate limit (chống spam)
        if (!CanRequestPasswordReset(user))
        {
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = $"Bạn đã yêu cầu reset password quá {MAX_RESET_REQUESTS_PER_DAY} lần trong ngày. Vui lòng thử lại sau."
            };
        }

        // Tạo mã reset 6 số
        var resetCode = Generate6DigitCode();
        var hashedCode = HashToken(resetCode);

        // Lưu vào database
        user.PasswordResetToken = hashedCode;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(RESET_TOKEN_EXPIRY_MINUTES);
        user.PasswordResetRequestCount = GetTodayResetCount(user) + 1;
        user.LastPasswordResetRequest = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Gửi email với mã reset
        await _emailService.SendPasswordResetCodeAsync(user.Email, user.FullName ?? user.Username, resetCode);

        var isDevelopment = _configuration.GetValue<bool>("IsDevelopment", false);

        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "Mã reset password đã được gửi đến email của bạn.",
            ResetToken = isDevelopment ? resetCode : null, // Chỉ trả code trong dev hoặc khi đang fake
            ExpiresInMinutes = RESET_TOKEN_EXPIRY_MINUTES
        };
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Tìm user
        var user = await FindUserByEmailOrUsernameAsync(request.EmailOrUsername);

        if (user == null)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Mã không hợp lệ hoặc đã hết hạn."
            };
        }

        // Kiểm tra user có active không
        if (!user.IsActive)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Tài khoản đã bị vô hiệu hóa."
            };
        }

        // Validate mã reset
        if (!IsValidResetToken(user, request.ResetToken))
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Mã không hợp lệ hoặc đã hết hạn."
            };
        }

        // Hash và lưu password mới
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Clear reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Gửi email thông báo password đã được đổi
        if (!string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName ?? user.Username);
        }

        return new ResetPasswordResponse
        {
            Success = true,
            Message = "Đổi mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới."
        };
    }

    public async Task<bool> ValidateResetTokenAsync(string emailOrUsername, string token)
    {
        var user = await FindUserByEmailOrUsernameAsync(emailOrUsername);
        if (user == null) return false;

        return IsValidResetToken(user, token);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Tìm user theo email hoặc username
    /// </summary>
    private async Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername)
    {
        if (string.IsNullOrWhiteSpace(emailOrUsername))
            return null;

        var normalized = emailOrUsername.Trim().ToLower();

        return await _context.Users
            .FirstOrDefaultAsync(u => 
                u.Username.ToLower() == normalized || 
                (u.Email != null && u.Email.ToLower() == normalized));
    }

    /// <summary>
    /// Tạo reset token (raw và hashed)
    /// </summary>
    private static (string RawToken, string HashedToken) GenerateResetToken()
    {
        // Tạo token 32 bytes random
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // Hash token trước khi lưu vào database
        var hashedToken = HashToken(rawToken);

        return (rawToken, hashedToken);
    }

    /// <summary>
    /// Hash token với SHA256
    /// </summary>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Tạo mã 6 số ngẫu nhiên
    /// </summary>
    private static string Generate6DigitCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    /// <summary>
    /// Kiểm tra reset token có hợp lệ không
    /// </summary>
    private static bool IsValidResetToken(User user, string rawToken)
    {
        // Kiểm tra token đã được set chưa
        if (string.IsNullOrEmpty(user.PasswordResetToken))
            return false;

        // Kiểm tra hết hạn
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return false;

        // So sánh hash
        var hashedInput = HashToken(rawToken);
        return hashedInput == user.PasswordResetToken;
    }

    /// <summary>
    /// Kiểm tra verification code có hợp lệ không
    /// </summary>
    private static bool IsValidVerificationCode(User user, string code)
    {
        if (string.IsNullOrEmpty(user.EmailVerificationCode))
            return false;

        if (user.EmailVerificationCodeExpiry == null || user.EmailVerificationCodeExpiry < DateTime.UtcNow)
            return false;

        var hashedInput = HashToken(code);
        return hashedInput == user.EmailVerificationCode;
    }

    /// <summary>
    /// Kiểm tra user có thể request reset password không (rate limit)
    /// </summary>
    private static bool CanRequestPasswordReset(User user)
    {
        var todayCount = GetTodayResetCount(user);
        return todayCount < MAX_RESET_REQUESTS_PER_DAY;
    }

    /// <summary>
    /// Kiểm tra user có thể request email verification không (rate limit)
    /// </summary>
    private static bool CanRequestEmailVerification(User user)
    {
        var todayCount = GetTodayEmailVerificationCount(user);
        return todayCount < MAX_EMAIL_VERIFICATION_REQUESTS_PER_DAY;
    }

    /// <summary>
    /// Lấy số lần request reset trong ngày hôm nay
    /// </summary>
    private static int GetTodayResetCount(User user)
    {
        if (user.LastPasswordResetRequest == null)
            return 0;

        // Reset count nếu ngày mới
        if (user.LastPasswordResetRequest.Value.Date < DateTime.UtcNow.Date)
            return 0;

        return user.PasswordResetRequestCount;
    }

    /// <summary>
    /// Lấy số lần request email verification trong ngày hôm nay
    /// </summary>
    private static int GetTodayEmailVerificationCount(User user)
    {
        if (user.LastEmailVerificationRequest == null)
            return 0;

        if (user.LastEmailVerificationRequest.Value.Date < DateTime.UtcNow.Date)
            return 0;

        return user.EmailVerificationRequestCount;
    }

    #endregion
}