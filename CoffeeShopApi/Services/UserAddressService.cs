using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopApi.Services;

public interface IUserAddressService
{
    /// <summary>
    /// Lấy tất cả địa chỉ của user
    /// </summary>
    Task<UserAddressListResponse> GetByUserIdAsync(int userId);

    /// <summary>
    /// Lấy địa chỉ theo ID (chỉ trả về nếu thuộc về user)
    /// </summary>
    Task<UserAddressResponse?> GetByIdAsync(int addressId, int userId);

    /// <summary>
    /// Lấy địa chỉ mặc định của user
    /// </summary>
    Task<UserAddressResponse?> GetDefaultAddressAsync(int userId);

    /// <summary>
    /// Thêm địa chỉ mới cho user
    /// </summary>
    Task<UserAddressResponse> CreateAsync(int userId, CreateUserAddressRequest request);

    /// <summary>
    /// Cập nhật địa chỉ (chỉ cho phép user sở hữu)
    /// </summary>
    Task<UserAddressResponse?> UpdateAsync(int addressId, int userId, UpdateUserAddressRequest request);

    /// <summary>
    /// Đặt địa chỉ làm mặc định (unset các địa chỉ khác)
    /// </summary>
    Task<UserAddressResponse?> SetDefaultAsync(int addressId, int userId);

    /// <summary>
    /// Xóa địa chỉ (chỉ cho phép user sở hữu)
    /// </summary>
    Task<bool> DeleteAsync(int addressId, int userId);

    /// <summary>
    /// Kiểm tra địa chỉ có thuộc về user không
    /// </summary>
    Task<bool> IsAddressOwnedByUserAsync(int addressId, int userId);

    /// <summary>
    /// Lấy entity UserAddress (dùng cho snapshotting trong OrderService)
    /// </summary>
    Task<UserAddress?> GetAddressEntityAsync(int addressId, int userId);
}

public class UserAddressService : IUserAddressService
{
    private readonly AppDbContext _context;

    public UserAddressService(AppDbContext context)
    {
        _context = context;
    }

    #region Query Methods

    public async Task<UserAddressListResponse> GetByUserIdAsync(int userId)
    {
        var addresses = await _context.UserAddresses
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.IsDefault)
            .ThenByDescending(ua => ua.CreatedAt)
            .ToListAsync();

        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);

        return new UserAddressListResponse
        {
            Addresses = addresses.Select(MapToResponse).ToList(),
            TotalCount = addresses.Count,
            DefaultAddressId = defaultAddress?.Id
        };
    }

    public async Task<UserAddressResponse?> GetByIdAsync(int addressId, int userId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);

        return address == null ? null : MapToResponse(address);
    }

    public async Task<UserAddressResponse?> GetDefaultAddressAsync(int userId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsDefault);

        // Nếu không có default, lấy địa chỉ đầu tiên
        if (address == null)
        {
            address = await _context.UserAddresses
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.CreatedAt)
                .FirstOrDefaultAsync();
        }

        return address == null ? null : MapToResponse(address);
    }

    public async Task<bool> IsAddressOwnedByUserAsync(int addressId, int userId)
    {
        return await _context.UserAddresses
            .AnyAsync(ua => ua.Id == addressId && ua.UserId == userId);
    }

    public async Task<UserAddress?> GetAddressEntityAsync(int addressId, int userId)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);
    }

    #endregion

    #region Command Methods

    public async Task<UserAddressResponse> CreateAsync(int userId, CreateUserAddressRequest request)
    {
        // Validate user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new ArgumentException($"User với ID {userId} không tồn tại");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Nếu đặt làm default, unset các địa chỉ khác
            if (request.IsDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
            }
            else
            {
                // Nếu đây là địa chỉ đầu tiên, tự động đặt làm default
                var hasAnyAddress = await _context.UserAddresses.AnyAsync(ua => ua.UserId == userId);
                if (!hasAnyAddress)
                {
                    request.IsDefault = true;
                }
            }

            var address = new UserAddress
            {
                UserId = userId,
                RecipientName = request.RecipientName,
                PhoneNumber = request.PhoneNumber,
                AddressLine = request.AddressLine,
                Label = request.Label,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return MapToResponse(address);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<UserAddressResponse?> UpdateAsync(int addressId, int userId, UpdateUserAddressRequest request)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);

        if (address == null)
            return null;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Nếu đặt làm default, unset các địa chỉ khác
            if (request.IsDefault == true && !address.IsDefault)
            {
                await UnsetDefaultAddressesAsync(userId);
                address.IsDefault = true;
            }
            else if (request.IsDefault == false)
            {
                address.IsDefault = false;
            }

            // Update các field khác nếu có
            if (!string.IsNullOrEmpty(request.RecipientName))
                address.RecipientName = request.RecipientName;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                address.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.AddressLine))
                address.AddressLine = request.AddressLine;
            if (request.Label != null)
                address.Label = request.Label;

            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(address);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<UserAddressResponse?> SetDefaultAsync(int addressId, int userId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);

        if (address == null)
            return null;

        if (address.IsDefault)
            return MapToResponse(address); // Đã là default rồi

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Unset tất cả các địa chỉ khác
            await UnsetDefaultAddressesAsync(userId);

            // Set địa chỉ này làm default
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(address);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int addressId, int userId)
    {
        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.Id == addressId && ua.UserId == userId);

        if (address == null)
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var wasDefault = address.IsDefault;

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            // Nếu xóa địa chỉ default, tự động set địa chỉ còn lại làm default
            if (wasDefault)
            {
                var nextAddress = await _context.UserAddresses
                    .Where(ua => ua.UserId == userId)
                    .OrderByDescending(ua => ua.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    nextAddress.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Unset tất cả địa chỉ default của user
    /// </summary>
    private async Task UnsetDefaultAddressesAsync(int userId)
    {
        var defaultAddresses = await _context.UserAddresses
            .Where(ua => ua.UserId == userId && ua.IsDefault)
            .ToListAsync();

        foreach (var addr in defaultAddresses)
        {
            addr.IsDefault = false;
            addr.UpdatedAt = DateTime.UtcNow;
        }

        if (defaultAddresses.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private static UserAddressResponse MapToResponse(UserAddress address)
    {
        return new UserAddressResponse
        {
            Id = address.Id,
            UserId = address.UserId,
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            AddressLine = address.AddressLine,
            Label = address.Label,
            IsDefault = address.IsDefault,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    #endregion
}
