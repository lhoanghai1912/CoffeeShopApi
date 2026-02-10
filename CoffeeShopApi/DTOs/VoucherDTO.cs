using CoffeeShopApi.Models;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopApi.DTOs;

#region Request DTOs

/// <summary>
/// Request to validate a voucher before checkout
/// </summary>
public class ValidateVoucherRequest
{
    [Required]
    public string VoucherCode { get; set; } = string.Empty;

    /// <summary>
    /// Order subtotal to check MinOrderValue constraint
    /// </summary>
    [Required]
    public decimal OrderSubTotal { get; set; }
}

/// <summary>
/// ⭐ Request to check voucher by ID (khi user chọn voucher từ danh sách)
/// </summary>
public class CheckVoucherRequest
{
    [Required]
    public int VoucherId { get; set; }

    /// <summary>
    /// Order subtotal để tính discount
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal OrderSubTotal { get; set; }
}

/// <summary>
/// Request to create a new voucher (Admin)
/// </summary>
public class CreateVoucherRequest
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "DiscountValue must be greater than 0")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinOrderValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True = Public voucher (anyone can use), False = Private voucher (assigned users only)
    /// </summary>
    public bool IsPublic { get; set; } = true;
}

/// <summary>
/// Request to update a voucher (Admin)
/// </summary>
public class UpdateVoucherRequest
{
    [MaxLength(200)]
    public string? Description { get; set; }

    public DiscountType? DiscountType { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinOrderValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimitPerUser { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// True = Public voucher, False = Private voucher
    /// </summary>
    public bool? IsPublic { get; set; }
}

/// <summary>
/// Request to assign a private voucher to specific users (Admin)
/// </summary>
public class AssignVoucherRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one voucher ID is required")]
    public List<int> VoucherIds { get; set; } = new();

    [Required]
    [MinLength(1, ErrorMessage = "At least one user ID is required")]
    public List<int> UserIds { get; set; } = new();

    [MaxLength(200)]
    public string? Note { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// Response for voucher validation
/// </summary>
public class VoucherValidationResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public VoucherResponse? Voucher { get; set; }

    /// <summary>
    /// Calculated discount amount based on the order subtotal
    /// </summary>
    public decimal CalculatedDiscount { get; set; }

    public static VoucherValidationResponse Valid(VoucherResponse voucher, decimal calculatedDiscount)
    {
        return new VoucherValidationResponse
        {
            IsValid = true,
            Voucher = voucher,
            CalculatedDiscount = calculatedDiscount
        };
    }

    public static VoucherValidationResponse Invalid(string errorMessage)
    {
        return new VoucherValidationResponse
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Full voucher details response
/// </summary>
public class VoucherResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public string DiscountTypeDisplay => DiscountType == DiscountType.FixedAmount ? "Fixed Amount" : "Percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Remaining uses (null if unlimited)
    /// </summary>
    public int? RemainingUses => UsageLimit.HasValue ? UsageLimit.Value - CurrentUsageCount : null;

    public static VoucherResponse FromEntity(Voucher voucher)
    {
        return new VoucherResponse
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            MinOrderValue = voucher.MinOrderValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            UsageLimit = voucher.UsageLimit,
            UsageLimitPerUser = voucher.UsageLimitPerUser,
            CurrentUsageCount = voucher.CurrentUsageCount,
            IsActive = voucher.IsActive,
            IsPublic = voucher.IsPublic,
            CreatedAt = voucher.CreatedAt
        };
    }
}

/// <summary>
/// Brief voucher info for list views
/// </summary>
public class VoucherSummaryResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountType DiscountType  { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public DateTime EndDate { get; set; }
    public int? RemainingUses { get; set; }

    public static VoucherSummaryResponse FromEntity(Voucher voucher)
    {
        
        return new VoucherSummaryResponse
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            IsActive = voucher.IsActive,
            IsPublic = voucher.IsPublic,
            EndDate = voucher.EndDate,
            RemainingUses = voucher.UsageLimit.HasValue
                ? voucher.UsageLimit.Value - voucher.CurrentUsageCount
                : null
        };
    }
}

/// <summary>
/// Response for user's assigned voucher
/// </summary>
public class UserVoucherResponse
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public string? VoucherDescription { get; set; }
    
    public bool IsUsed { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? Note { get; set; }
    public DateTime VoucherEndDate { get; set; }
    public bool IsExpired => DateTime.UtcNow > VoucherEndDate;

    public static UserVoucherResponse FromEntity(Models.UserVoucher userVoucher)
    {
        var voucher = userVoucher.Voucher;
       

        return new UserVoucherResponse
        {
            Id = userVoucher.Id,
            VoucherId = userVoucher.VoucherId,
            VoucherCode = voucher.Code,
            VoucherDescription = voucher.Description,
            
            IsUsed = userVoucher.IsUsed,
            AssignedAt = userVoucher.AssignedAt,
            UsedAt = userVoucher.UsedAt,
            Note = userVoucher.Note,
            VoucherEndDate = voucher.EndDate
        };
    }
}

#endregion
