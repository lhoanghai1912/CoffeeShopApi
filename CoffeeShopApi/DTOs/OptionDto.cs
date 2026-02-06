namespace CoffeeShopApi.DTOs;

public class OptionGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>
    /// [DEPENDENCY] Group này chỉ hiển thị khi OptionItem với Id này được chọn
    /// Null = luôn hiển thị
    /// </summary>
    public int? DependsOnOptionItemId { get; set; }

    public List<OptionItemDto> OptionItems { get; set; } = new();
}

public class OptionItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateOptionGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public int DisplayOrder { get; set; }
    public int? DependsOnOptionItemId { get; set; }
    public List<CreateOptionItemRequest> OptionItems { get; set; } = new();
}

public class CreateOptionItemRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}
