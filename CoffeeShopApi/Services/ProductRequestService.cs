using System.Text.Json;
using CoffeeShopApi.DTOs;

namespace CoffeeShopApi.Services;

public interface IProductRequestService
{
    T ParseFormData<T>(string formField) where T : class;
    List<string> ValidateProductRequest(CreateProductRequest request);
}

public class ProductRequestService : IProductRequestService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public T ParseFormData<T>(string formField) where T : class
    {
        if (string.IsNullOrWhiteSpace(formField))
            throw new ArgumentException("FormField is empty!");

        try
        {
            var data = JsonSerializer.Deserialize<T>(formField, JsonOptions);
            if (data == null)
                throw new ArgumentException("Cannot parse FormField!");
            return data;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"JSON parse error: {ex.Message}", ex);
        }
    }

    public List<string> ValidateProductRequest(CreateProductRequest request)
    {
        var errors = new List<string>();

        if (request.Category != null)
        {
            if (request.Category.Id == null)
                errors.Add("categoryId is required");
            else if (request.Category.Id <= 0)
                errors.Add("categoryId must be a positive integer");
        }

        // ⭐ Validate OptionGroups (chỉ cần check OptionGroupId)
        if (request.OptionGroups != null)
        {
            for (int i = 0; i < request.OptionGroups.Count; i++)
            {
                var og = request.OptionGroups[i];

                if (og.OptionGroupId <= 0)
                    errors.Add($"optionGroups[{i}].optionGroupId must be a positive integer");

                // Validate AllowedItemIds nếu có
                if (og.AllowedItemIds != null && og.AllowedItemIds.Any())
                {
                    if (og.AllowedItemIds.Any(id => id <= 0))
                        errors.Add($"optionGroups[{i}].allowedItemIds must contain positive integers only");
                }
            }
        }

        return errors;
    }
}
