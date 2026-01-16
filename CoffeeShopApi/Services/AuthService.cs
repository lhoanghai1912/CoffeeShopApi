using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    Task<User?> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Kiểm tra user có thể login không (active, credentials đúng)
    /// </summary>
    Task<(bool CanLogin, string? ErrorMessage, User? User)> ValidateLoginAsync(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IRoleService _roleService;

    public AuthService(AppDbContext context, IConfiguration configuration, IRoleService roleService)
    {
        _context = context;
        _configuration = configuration;
        _roleService = roleService;
    }

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

        // ✅ Kiểm tra user có active không
        if (!user.IsActive)
            return (false, "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ.", null);

        return (true, null, user);
    }

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

    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        // ✅ Kiểm tra username đã tồn tại
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return null; 

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

        var newUser = new User
        {
            Username = request.Username,
            Password = passwordHash,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };      

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }
}