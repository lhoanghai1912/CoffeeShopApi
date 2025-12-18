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

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return null;
        }

        RoleResponse? roleResponse = _roleService.ToRoleResponse(user.Role);

        // Lấy danh sách permissions của user
        var permissions = user.Role.RolePermissions
            .Select(rp => rp.Permission.Code)
            .ToList();

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
            new(ClaimTypes.Role, user.Role.Code), // Role claim (ADMIN, CUSTOMER, STAFF)
            new("roleId", user.Role.Id.ToString()),
            new("roleName", user.Role.Name)
        };

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
        };      

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }
    
    
}