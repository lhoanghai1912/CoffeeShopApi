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

        if (request.OptionGroups != null)
        {
            for (int i = 0; i < request.OptionGroups.Count; i++)
            {
                var og = request.OptionGroups[i];
                if (string.IsNullOrWhiteSpace(og.Name))
                    errors.Add($"optionGroups[{i}].name is required");
                
                if (og.OptionItems != null)
                {
                    for (int j = 0; j < og.OptionItems.Count; j++)
                    {
                        var oi = og.OptionItems[j];
                        if (string.IsNullOrWhiteSpace(oi.Name))
                            errors.Add($"optionGroups[{i}].optionItems[{j}].name is required");
                    }
                }
            }
        }

        return errors;
    }
}
