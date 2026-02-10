using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using CoffeeShopApi.Repositories;

namespace CoffeeShopApi.Services;

public interface IOptionGroupService
{
    Task<IEnumerable<OptionGroupDto>> GetAllAsync();
    Task<OptionGroupDto?> GetByIdAsync(int id);
    Task<OptionGroupDto> CreateAsync(CreateOptionGroupRequest request);
    Task<bool> UpdateAsync(int id, CreateOptionGroupRequest request);
    Task<bool> DeleteAsync(int id);
}

public class OptionGroupService : IOptionGroupService
{
    private readonly IOptionGroupRepository _repository;
    private readonly ILogger<OptionGroupService> _logger;

    public OptionGroupService(
        IOptionGroupRepository repository,
        ILogger<OptionGroupService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<OptionGroupDto>> GetAllAsync()
    {
        var optionGroups = await _repository.GetAllAsync();
        return optionGroups.Select(MapToDto);
    }

    public async Task<OptionGroupDto?> GetByIdAsync(int id)
    {
        var optionGroup = await _repository.GetByIdAsync(id);
        return optionGroup == null ? null : MapToDto(optionGroup);
    }

    public async Task<OptionGroupDto> CreateAsync(CreateOptionGroupRequest request)
    {
        var optionGroup = new OptionGroup
        {
            Name = request.Name,
            Description = request.Description,
            IsRequired = request.IsRequired,
            AllowMultiple = request.AllowMultiple,
            DisplayOrder = request.DisplayOrder,
            DependsOnOptionItemId = request.DependsOnOptionItemId,
            OptionItems = request.OptionItems.Select((item, index) => new OptionItem
            {
                Name = item.Name,
                PriceAdjustment = item.PriceAdjustment,
                IsDefault = item.IsDefault,
                DisplayOrder = item.DisplayOrder > 0 ? item.DisplayOrder : index
            }).ToList()
        };

        var created = await _repository.CreateAsync(optionGroup);
        _logger.LogInformation("Created OptionGroup: {Name} with {Count} items", created.Name, created.OptionItems.Count);
        
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, CreateOptionGroupRequest request)
    {
        var optionGroup = await _repository.GetByIdAsync(id);
        if (optionGroup == null)
        {
            _logger.LogWarning("OptionGroup {Id} not found for update", id);
            return false;
        }

        optionGroup.Name = request.Name;
        optionGroup.Description = request.Description;
        optionGroup.IsRequired = request.IsRequired;
        optionGroup.AllowMultiple = request.AllowMultiple;
        optionGroup.DisplayOrder = request.DisplayOrder;
        optionGroup.DependsOnOptionItemId = request.DependsOnOptionItemId;

        var success = await _repository.UpdateAsync(optionGroup);
        if (success)
        {
            _logger.LogInformation("Updated OptionGroup: {Id} - {Name}", id, optionGroup.Name);
        }
        
        return success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (success)
        {
            _logger.LogInformation("Deleted OptionGroup: {Id}", id);
        }
        else
        {
            _logger.LogWarning("OptionGroup {Id} not found for deletion", id);
        }
        
        return success;
    }

    private static OptionGroupDto MapToDto(OptionGroup optionGroup)
    {
        return new OptionGroupDto
        {
            Id = optionGroup.Id,
            Name = optionGroup.Name,
            Description = optionGroup.Description,
            IsRequired = optionGroup.IsRequired,
            AllowMultiple = optionGroup.AllowMultiple,
            DisplayOrder = optionGroup.DisplayOrder,
            DependsOnOptionItemId = optionGroup.DependsOnOptionItemId,
            OptionItems = optionGroup.OptionItems.Select(item => new OptionItemDto
            {
                Id = item.Id,
                Name = item.Name,
                PriceAdjustment = item.PriceAdjustment,
                IsDefault = item.IsDefault,
                DisplayOrder = item.DisplayOrder
            }).OrderBy(item => item.DisplayOrder).ToList()
        };
    }
}
