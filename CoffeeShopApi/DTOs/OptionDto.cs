namespace CoffeeShopApi.DTOs;

public class OptionGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public int DisplayOrder { get; set; }
    public int FatherId { get; set; }
    public List<OptionItemDto> OptionItems { get; set; } = new();
}

public class OptionItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public int FatherId { get; set; }
}

public class CreateOptionGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public int DisplayOrder { get; set; }
    public int FatherId { get; set; }
    public List<CreateOptionItemRequest> OptionItems { get; set; } = new();
}

public class CreateOptionItemRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public int FatherId { get; set; }
}
