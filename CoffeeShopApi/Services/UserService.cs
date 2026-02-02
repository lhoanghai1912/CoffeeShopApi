using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using CoffeeShopApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface IUserService
{
    /// <summary>
    /// Lấy thông tin profile của user
    /// </summary>
    Task<UserProfileResponse?> GetProfileAsync(int userId);
    
    /// <summary>
    /// Lấy profile kèm thống kê order
    /// </summary>
    Task<UserProfileResponse?> GetProfileWithStatsAsync(int userId);
    
    /// <summary>
    /// Cập nhật thông tin profile (FullName, PhoneNumber, Email)
    /// </summary>
    Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    
    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    
    /// <summary>
    /// Deactivate user (soft delete)
    /// </summary>
    Task<bool> DeactivateUserAsync(int userId, string? reason = null);
    
    /// <summary>
    /// Reactivate user
    /// </summary>
    Task<bool> ReactivateUserAsync(int userId);
    
    /// <summary>
    /// Kiểm tra user có active không
    /// </summary>
    Task<bool> IsUserActiveAsync(int userId);
    
    /// <summary>
    /// Lấy user theo ID (internal use)
    /// </summary>
    Task<User?> GetByIdAsync(int userId);
    
    /// <summary>
    /// Kiểm tra email đã tồn tại chưa (trừ user hiện tại)
    /// </summary>
    Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    #region Profile Methods

    public async Task<UserProfileResponse?> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return MapToProfileResponse(user);
    }

    public async Task<UserProfileResponse?> GetProfileWithStatsAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var response = MapToProfileResponse(user);

        // Load order stats
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Select(o => new { o.Status, o.FinalAmount })
            .ToListAsync();

        response.OrderStats = new UserOrderStats
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.Paid),
            TotalSpent = orders.Where(o => o.Status == OrderStatus.Paid).Sum(o => o.FinalAmount)
        };

        // Load addresses
        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        response.Addresses = addresses.Select(a => new UserAddressResponse
        {
            Id = a.Id,
            UserId = a.UserId,
            RecipientName = a.RecipientName,
            PhoneNumber = a.PhoneNumber,
            AddressLine = a.AddressLine,
            Label = a.Label,
            IsDefault = a.IsDefault,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return response;
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        // Validate email uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await IsEmailExistsAsync(request.Email, userId);
            if (emailExists)
            {
                throw new InvalidOperationException("Email đã được sử dụng bởi tài khoản khác");
            }
        }

        // Update fields (only if provided)
        if (request.FullName != null)
            user.FullName = request.FullName;
        
        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;
        
        if (request.Email != null)
            user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProfileResponse(user);
    }

    #endregion

    #region Password Methods

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Không tìm thấy user");

        if (!user.IsActive)
            return (false, "Tài khoản đã bị vô hiệu hóa");

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            return (false, "Mật khẩu cũ không đúng");

        // Check new password is different from old
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password))
            return (false, "Mật khẩu mới phải khác mật khẩu cũ");

        // Hash and save new password
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Invalidate all existing tokens/sessions for this user
        // Khi có Redis/Token blacklist, implement logout all sessions here

        return (true, "Đổi mật khẩu thành công");
    }

    #endregion

    #region User Lifecycle Methods

    public async Task<bool> DeactivateUserAsync(int userId, string? reason = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // TODO: Log deactivation reason to audit table
        // await _auditService.LogAsync(userId, "DEACTIVATE", reason);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserActiveAsync(int userId)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == userId && u.IsActive);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);

        return await query.AnyAsync();
    }

    #endregion

    #region Private Helpers

    private static UserProfileResponse MapToProfileResponse(User user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    #endregion
}
