using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
    Task<(User?, string)> RegisterAsync(RegisterRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return null;
        }
        
        // var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId ,
            Token = token 
            
        };
    }

    // Hàm phụ trợ để sinh chuỗi JWT
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString()),
            new("fullName", user.FullName ?? ""),
            
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2), // Token sống trong 2 giờ
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        //token fullname va id
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<(User?,string)> RegisterAsync(RegisterRequest request)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        int roleIdToUse = request.RoleId ?? 1;
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return (null, "Tên tài khoản đã được sử dụng");

        if (string.IsNullOrEmpty(request.FullName))
        {
            return (null, "Nhập fullName");
        }
        
        var newUser = new User
        {
            Username = request.Username,
            Password = passwordHash,
            FullName = request.FullName,
           PhoneNumber = request.PhoneNumber,
           RoleId =  roleIdToUse,
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return (newUser,"");
    }
}