using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace CoffeeShopApi.Services;

public interface IVoucherService
{
    /// <summary>
    /// Validate a voucher code for a user and order subtotal (pre-checkout check)
    /// </summary>
    Task<VoucherValidationResponse> ValidateVoucherAsync(string voucherCode, int userId, decimal orderSubTotal);

    /// <summary>
    /// Calculate the discount amount for a voucher and order subtotal
    /// </summary>
    decimal CalculateDiscount(Voucher voucher, decimal orderSubTotal);

    /// <summary>
    /// Apply a voucher to an order - increments usage counts atomically
    /// Returns the voucher entity if successful, null if failed
    /// </summary>
    Task<Voucher?> ApplyVoucherAsync(int voucherId, int userId);

    /// <summary>
    /// Rollback voucher usage - decrements usage counts (for failed transactions)
    /// </summary>
    Task RollbackVoucherUsageAsync(int voucherId, int userId);

    /// <summary>
    /// Get voucher by code
    /// </summary>
    Task<Voucher?> GetByCodeAsync(string code);

    /// <summary>
    /// Get voucher by ID
    /// </summary>
    Task<Voucher?> GetByIdAsync(int id);

    /// <summary>
    /// Get all vouchers with pagination (Admin)
    /// </summary>
    Task<PaginatedResponse<VoucherSummaryResponse>> GetPagedAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null,
        string? orderBy = null,
        string? filter = null);

    /// <summary>
    /// Get all vouchers (Admin) - No pagination
    /// </summary>
    Task<List<VoucherSummaryResponse>> GetAllAsync(bool? isActive = null);

    /// <summary>
    /// Create a new voucher (Admin)
    /// </summary>
    Task<VoucherResponse> CreateAsync(CreateVoucherRequest request);

    /// <summary>
    /// Update a voucher (Admin)
    /// </summary>
    Task<VoucherResponse?> UpdateAsync(int id, UpdateVoucherRequest request);

    /// <summary>
    /// Delete a voucher (Admin) - soft delete by setting IsActive = false
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Assign private vouchers to specific users (Admin)
    /// </summary>
    Task<int> AssignVoucherToUsersAsync(List<int> voucherIds, List<int> userIds, string? note = null);

    /// <summary>
    /// Get all vouchers assigned to users (including used/unused)
    /// </summary>
    Task<List<UserVoucherResponse>> GetUserVouchersAsync(int userId, bool? isUsed = null);

    /// <summary>
    /// Get users assigned to a specific voucher (Admin)
    /// </summary>
    Task<List<UserVoucherResponse>> GetVoucherAssignmentsAsync(int voucherId);

    /// <summary>
    /// Get all available vouchers for a user (public vouchers + assigned private vouchers)
    /// </summary>
    Task<List<VoucherSummaryResponse>> GetAvailableVouchersForUserAsync(int userId, bool? onlyUnused = null);

    /// <summary>
    /// Tự động cập nhật IsActive của voucher dựa trên StartDate và EndDate
    /// </summary>
    Task<int> UpdateVoucherActiveStatusAsync();
}

public class VoucherService : IVoucherService
{
    private readonly AppDbContext _context;

    public VoucherService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
    /// </summary>
    private static DateTime GetVietnamTime()
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
    }

    #region Feature A: Validate Voucher

    public async Task<VoucherValidationResponse> ValidateVoucherAsync(string voucherCode, int userId, decimal orderSubTotal)
    {
        // 1. Check if code exists and is active
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code.ToUpper() == voucherCode.ToUpper());

        if (voucher == null)
        {
            return VoucherValidationResponse.Invalid("Voucher code not found");
        }

        if (!voucher.IsActive)
        {
            return VoucherValidationResponse.Invalid("This voucher is no longer active");
        }

        // 2. Check date range
        var now = GetVietnamTime();
        if (now < voucher.StartDate)
        {
            return VoucherValidationResponse.Invalid($"This voucher is not valid yet. Valid from {voucher.StartDate:dd/MM/yyyy}");
        }

        if (now > voucher.EndDate)
        {
            return VoucherValidationResponse.Invalid("This voucher has expired");
        }

        // 3. Check minimum order value
        if (voucher.MinOrderValue.HasValue && orderSubTotal < voucher.MinOrderValue.Value)
        {
            return VoucherValidationResponse.Invalid(
                $"Order subtotal must be at least {voucher.MinOrderValue.Value:N0}đ to use this voucher");
        }

        // 4. Global usage limit check
        if (voucher.UsageLimit.HasValue && voucher.CurrentUsageCount >= voucher.UsageLimit.Value)
        {
            return VoucherValidationResponse.Invalid("This voucher has reached its usage limit");
        }

        // 5. Private voucher check - must be assigned to user and not used yet
        if (!voucher.IsPublic)
        {
            var userVoucher = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.VoucherId == voucher.Id && uv.UserId == userId);

            if (userVoucher == null)
            {
                return VoucherValidationResponse.Invalid("This voucher is not assigned to you");
            }

            if (userVoucher.IsUsed)
            {
                return VoucherValidationResponse.Invalid("You have already used this private voucher");
            }
        }

        // 6. Per-user usage limit check (for public vouchers)
        if (voucher.IsPublic && voucher.UsageLimitPerUser.HasValue)
        {
            var userUsage = await _context.VoucherUsages
                .FirstOrDefaultAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == userId);

            if (userUsage != null && userUsage.UsageCount >= voucher.UsageLimitPerUser.Value)
            {
                return VoucherValidationResponse.Invalid(
                    $"You have already used this voucher {voucher.UsageLimitPerUser.Value} time(s)");
            }
        }

        // All validations passed - calculate discount
        var discountAmount = CalculateDiscount(voucher, orderSubTotal);
        var voucherResponse = VoucherResponse.FromEntity(voucher);

        return VoucherValidationResponse.Valid(voucherResponse, discountAmount);
    }

    #endregion

    #region Feature B: Calculate Discount

    public decimal CalculateDiscount(Voucher voucher, decimal orderSubTotal)
    {
        decimal discountAmount;

        if (voucher.DiscountType == DiscountType.FixedAmount)
        {
            // Fixed amount discount
            discountAmount = voucher.DiscountValue;
        }
        else
        {
            // Percentage discount
            discountAmount = orderSubTotal * (voucher.DiscountValue / 100);

            // Apply max discount cap if set
            if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
            {
                discountAmount = voucher.MaxDiscountAmount.Value;
            }
        }

        // Ensure discount doesn't exceed order subtotal (prevent negative totals)
        if (discountAmount > orderSubTotal)
        {
            discountAmount = orderSubTotal;
        }

        return Math.Round(discountAmount, 2);
    }

    #endregion

    #region Feature C: Apply Voucher (Atomic Update)

    public async Task<Voucher?> ApplyVoucherAsync(int voucherId, int userId)
    {
        // Use optimistic concurrency with atomic update
        // This prevents race conditions when multiple users try to use the same voucher

        var voucher = await _context.Vouchers.FindAsync(voucherId);
        if (voucher == null) return null;

        // Atomic increment of global usage count
        // Uses raw SQL to ensure atomicity at database level
        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE Vouchers 
               SET CurrentUsageCount = CurrentUsageCount + 1, 
                   UpdatedAt = {DateTime.UtcNow}
               WHERE Id = {voucherId} 
               AND IsActive = 1
               AND (UsageLimit IS NULL OR CurrentUsageCount < UsageLimit)");

        if (rowsAffected == 0)
        {
            // Voucher limit reached or voucher is inactive
            return null;
        }

        // For private vouchers: mark the UserVoucher as used
        if (!voucher.IsPublic)
        {
            var userVoucher = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.VoucherId == voucherId && uv.UserId == userId && !uv.IsUsed);

            if (userVoucher != null)
            {
                userVoucher.IsUsed = true;
                userVoucher.UsedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // For public vouchers: update or create user-specific usage record
            var userUsage = await _context.VoucherUsages
                .FirstOrDefaultAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId);

            if (userUsage == null)
            {
                userUsage = new VoucherUsage
                {
                    VoucherId = voucherId,
                    UserId = userId,
                    UsageCount = 1,
                    LastUsedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.VoucherUsages.Add(userUsage);
            }
            else
            {
                userUsage.UsageCount++;
                userUsage.LastUsedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        // Refresh voucher from database to get updated count
        await _context.Entry(voucher).ReloadAsync();

        return voucher;
    }

    public async Task RollbackVoucherUsageAsync(int voucherId, int userId)
    {
        var voucher = await _context.Vouchers.FindAsync(voucherId);
        if (voucher == null) return;

        // Rollback global usage count
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE Vouchers 
               SET CurrentUsageCount = CASE WHEN CurrentUsageCount > 0 THEN CurrentUsageCount - 1 ELSE 0 END,
                   UpdatedAt = {DateTime.UtcNow}
               WHERE Id = {voucherId}");

        if (!voucher.IsPublic)
        {
            // For private vouchers: reset IsUsed flag
            var userVoucher = await _context.UserVouchers
                .FirstOrDefaultAsync(uv => uv.VoucherId == voucherId && uv.UserId == userId && uv.IsUsed);

            if (userVoucher != null)
            {
                userVoucher.IsUsed = false;
                userVoucher.UsedAt = null;
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // For public vouchers: rollback user-specific usage count
            var userUsage = await _context.VoucherUsages
                .FirstOrDefaultAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId);

            if (userUsage != null)
            {
                if (userUsage.UsageCount <= 1)
                {
                    _context.VoucherUsages.Remove(userUsage);
                }
                else
                {
                    userUsage.UsageCount--;
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    #endregion

    #region CRUD Operations (Admin)

    public async Task<Voucher?> GetByCodeAsync(string code)
    {
        return await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code.ToUpper() == code.ToUpper());
    }

    public async Task<Voucher?> GetByIdAsync(int id)
    {
        return await _context.Vouchers.FindAsync(id);
    }

    public async Task<List<VoucherSummaryResponse>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.Vouchers.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(v => v.IsActive == isActive.Value);
        }

        var vouchers = await query
            .OrderBy(v => v.Id)
            .ToListAsync();

        return vouchers.Select(VoucherSummaryResponse.FromEntity).ToList();
    }

    public async Task<PaginatedResponse<VoucherSummaryResponse>> GetPagedAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null,
        string? orderBy = null,
        string? filter = null)
    {
        var query = _context.Vouchers.AsQueryable();

        // Apply custom filter logic (fallback nếu Gridify không hoạt động)
        if (!string.IsNullOrWhiteSpace(filter))
        {
            // Parse filter manually: IsActive=true,IsPublic=false hoặc IsActive=true;IsPublic=false
            var filterParts = filter.Split(new[] { ',', ';', '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in filterParts)
            {
                var trimmedPart = part.Trim().Replace("&", ""); // Remove extra & if exists

                if (trimmedPart.StartsWith("IsActive=", StringComparison.OrdinalIgnoreCase))
                {
                    var value = trimmedPart.Split('=')[1].Trim();
                    if (bool.TryParse(value, out bool isActive))
                    {
                        query = query.Where(v => v.IsActive == isActive);
                    }
                }
                else if (trimmedPart.StartsWith("IsPublic=", StringComparison.OrdinalIgnoreCase))
                {
                    var value = trimmedPart.Split('=')[1].Trim();
                    if (bool.TryParse(value, out bool isPublic))
                    {
                        query = query.Where(v => v.IsPublic == isPublic);
                    }
                }
                // Có thể thêm các filter khác tại đây
            }
        }

        // Apply search (Code or Description)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => 
                v.Code.Contains(search) || 
                (v.Description != null && v.Description.Contains(search)));
        }

        // Always order by Id ascending to ensure consistent ordering
        query = query.OrderBy(v => v.Id);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var vouchers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = vouchers.Select(VoucherSummaryResponse.FromEntity).ToList();

        return new PaginatedResponse<VoucherSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<VoucherResponse> CreateAsync(CreateVoucherRequest request)
    {
        // Check for duplicate code
        var existingVoucher = await GetByCodeAsync(request.Code);
        if (existingVoucher != null)
        {
            throw new InvalidOperationException($"Voucher code '{request.Code}' already exists");
        }

        var voucher = new Voucher
        {
            Code = request.Code.ToUpper(),
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            UsageLimitPerUser = request.UsageLimitPerUser,
            IsActive = request.IsActive,
            IsPublic = request.IsPublic,
            CurrentUsageCount = 0,
            CreatedAt = GetVietnamTime(),
            UpdatedAt = GetVietnamTime()
        };

        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();

        return VoucherResponse.FromEntity(voucher);
    }

    public async Task<VoucherResponse?> UpdateAsync(int id, UpdateVoucherRequest request)
    {
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher == null) return null;

        // Update only provided fields
        if (request.Description != null)
            voucher.Description = request.Description;

        if (request.DiscountType.HasValue)
            voucher.DiscountType = request.DiscountType.Value;

        if (request.DiscountValue.HasValue)
            voucher.DiscountValue = request.DiscountValue.Value;

        if (request.MinOrderValue.HasValue)
            voucher.MinOrderValue = request.MinOrderValue;

        if (request.MaxDiscountAmount.HasValue)
            voucher.MaxDiscountAmount = request.MaxDiscountAmount;

        if (request.StartDate.HasValue)
            voucher.StartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            voucher.EndDate = request.EndDate.Value;

        if (request.UsageLimit.HasValue)
            voucher.UsageLimit = request.UsageLimit;

        if (request.UsageLimitPerUser.HasValue)
            voucher.UsageLimitPerUser = request.UsageLimitPerUser;

        if (request.IsActive.HasValue)
            voucher.IsActive = request.IsActive.Value;

        if (request.IsPublic.HasValue)
            voucher.IsPublic = request.IsPublic.Value;

        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return VoucherResponse.FromEntity(voucher);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher == null) return false;

        // Soft delete - just deactivate
        voucher.IsActive = false;
        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Private Voucher Assignment

    public async Task<int> AssignVoucherToUsersAsync(List<int> voucherIds, List<int> userIds, string? note = null)
    {
        try
        {
            // Lấy tất cả voucher cần gán
            var vouchers = await _context.Vouchers
                .Where(v => voucherIds.Contains(v.Id) && !v.IsPublic && v.IsActive)
                .ToListAsync();

            if (!vouchers.Any())
                throw new ArgumentException("Không tìm thấy voucher hợp lệ để gán");

            // Kiểm tra user IDs có tồn tại không
            var existingUserIds = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var invalidUserIds = userIds.Except(existingUserIds).ToList();
            if (invalidUserIds.Any())
                throw new ArgumentException($"User IDs không tồn tại: {string.Join(", ", invalidUserIds)}");

            // Lấy các assignment đã tồn tại để tránh trùng lặp
            var existingAssignments = await _context.UserVouchers
                .Where(uv => voucherIds.Contains(uv.VoucherId) && userIds.Contains(uv.UserId))
                .Select(uv => new { uv.VoucherId, uv.UserId })
                .ToListAsync();

            var newAssignments = new List<UserVoucher>();
            foreach (var voucher in vouchers)
            {
                foreach (var userId in userIds)
                {
                    bool alreadyAssigned = existingAssignments.Any(ea => ea.VoucherId == voucher.Id && ea.UserId == userId);
                    if (!alreadyAssigned)
                    {
                        newAssignments.Add(new UserVoucher
                        {
                            VoucherId = voucher.Id,
                            UserId = userId,
                            IsUsed = false,
                            AssignedAt = GetVietnamTime(),
                            Note = note
                        });
                    }
                }
            }

            if (!newAssignments.Any())
                return 0;

            await _context.UserVouchers.AddRangeAsync(newAssignments);
            await _context.SaveChangesAsync();

            return newAssignments.Count;
        }
        catch (DbUpdateException ex)
        {
            // Log chi tiết lỗi database
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {innerMessage}", ex);
        }
    }

    public async Task<List<UserVoucherResponse>> GetUserVouchersAsync(int userId, bool? isUsed = null)
    {
        var query = _context.UserVouchers
            .Include(uv => uv.Voucher)
            .Where(uv => uv.UserId == userId && uv.Voucher.IsActive);

        if (isUsed.HasValue)
        {
            query = query.Where(uv => uv.IsUsed == isUsed.Value);
        }

        var userVouchers = await query
            .OrderBy(uv => uv.Id)
            .ToListAsync();

        return userVouchers.Select(UserVoucherResponse.FromEntity).ToList();
    }

    public async Task<List<VoucherSummaryResponse>> GetAvailableVouchersForUserAsync(int userId, bool? onlyUnused = null)
    {
        var now = GetVietnamTime();
        var results = new List<VoucherSummaryResponse>();

        // 1. Lấy tất cả public vouchers (active, chưa hết hạn, còn lượt)
        var publicVouchers = await _context.Vouchers
            .Where(v => v.IsPublic
                     && v.IsActive
                     && v.EndDate >= now
                     && v.StartDate <= now
                     && (v.UsageLimit == null || v.CurrentUsageCount < v.UsageLimit))
            .ToListAsync();

        // 2. Lấy private vouchers đã được gán cho user
        var userVouchersQuery = _context.UserVouchers
            .Include(uv => uv.Voucher)
            .Where(uv => uv.UserId == userId
                      && !uv.Voucher.IsPublic
                      && uv.Voucher.IsActive
                      && uv.Voucher.EndDate >= now);

        // Nếu chỉ lấy voucher chưa dùng
        if (onlyUnused == true)
        {
            userVouchersQuery = userVouchersQuery.Where(uv => !uv.IsUsed);
        }

        var userVouchers = await userVouchersQuery.ToListAsync();
        var privateVouchers = userVouchers.Select(uv => uv.Voucher).ToList();

        // 3. Merge và map sang response
        var allVouchers = publicVouchers.Concat(privateVouchers).Distinct();
        results = allVouchers.Select(VoucherSummaryResponse.FromEntity).ToList();

        // Ensure results always sorted by Id ascending
        return results.OrderBy(v => v.Id).ToList();
    }

    public async Task<List<UserVoucherResponse>> GetVoucherAssignmentsAsync(int voucherId)
    {
        var userVouchers = await _context.UserVouchers
            .Include(uv => uv.Voucher)
            .Include(uv => uv.User)
            .Where(uv => uv.VoucherId == voucherId)
            .OrderBy(uv => uv.Id)
            .ToListAsync();

        return userVouchers.Select(UserVoucherResponse.FromEntity).ToList();
    }

    #endregion

    #region Auto update voucher active

    

    /// <summary>
    /// Tự động cập nhật IsActive của voucher dựa trên StartDate và EndDate
    /// Logic:
    /// - Nếu now < StartDate: IsActive = false (chưa đến thời gian)
    /// - Nếu StartDate <= now <= EndDate: IsActive = true (trong thời gian hiệu lực)
    /// - Nếu now > EndDate: IsActive = false (đã hết hạn)
    /// </summary>
    /// <returns>Số lượng voucher đã được cập nhật</returns>
    public async Task<int> UpdateVoucherActiveStatusAsync()
    {
        var now = GetVietnamTime();
        var updateCount = 0;

        // 1. Deactivate vouchers chưa đến thời gian (now < StartDate AND IsActive = true)
        var notYetActiveVouchers = await _context.Vouchers
            .Where(v => v.IsActive && now < v.StartDate)
            .ToListAsync();

        foreach (var voucher in notYetActiveVouchers)
        {
            voucher.IsActive = false;
            voucher.UpdatedAt = GetVietnamTime();
            updateCount++;
        }

        // 2. Activate vouchers trong thời gian hiệu lực (StartDate <= now <= EndDate AND IsActive = false)
        var shouldBeActiveVouchers = await _context.Vouchers
            .Where(v => !v.IsActive 
                     && v.StartDate <= now 
                     && v.EndDate >= now)
            .ToListAsync();

        foreach (var voucher in shouldBeActiveVouchers)
        {
            voucher.IsActive = true;
            voucher.UpdatedAt = GetVietnamTime();
            updateCount++;
        }

        // 3. Deactivate vouchers đã hết hạn (now > EndDate AND IsActive = true)
        var expiredVouchers = await _context.Vouchers
            .Where(v => v.IsActive && now > v.EndDate)
            .ToListAsync();

        foreach (var voucher in expiredVouchers)
        {
            voucher.IsActive = false;
            voucher.UpdatedAt = GetVietnamTime();
            updateCount++;
        }

        if (updateCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return updateCount;
    }
}
    #endregion
