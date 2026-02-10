using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using CoffeeShopApi.Repositories;

namespace CoffeeShopApi.Services;

public interface IOptionItemService
{
    Task<IEnumerable<OptionItemDto>> GetAllAsync();
    Task<IEnumerable<OptionItemDto>> GetByOptionGroupIdAsync(int optionGroupId);
    Task<OptionItemDto?> GetByIdAsync(int id);
    Task<OptionItemDto> CreateAsync(int optionGroupId, CreateOptionItemRequest request);
    Task<bool> UpdateAsync(int id, CreateOptionItemRequest request);
    Task<bool> DeleteAsync(int id);
}

public class OptionItemService : IOptionItemService
{
    private readonly IOptionItemRepository _repository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly ILogger<OptionItemService> _logger;

    public OptionItemService(
        IOptionItemRepository repository,
        IOptionGroupRepository optionGroupRepository,
        ILogger<OptionItemService> logger)
    {
        _repository = repository;
        _optionGroupRepository = optionGroupRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<OptionItemDto>> GetAllAsync()
    {
        var optionItems = await _repository.GetAllAsync();
        return optionItems.Select(MapToDto);
    }

    public async Task<IEnumerable<OptionItemDto>> GetByOptionGroupIdAsync(int optionGroupId)
    {
        var optionItems = await _repository.GetByOptionGroupIdAsync(optionGroupId);
        return optionItems.Select(MapToDto);
    }

    public async Task<OptionItemDto?> GetByIdAsync(int id)
    {
        var optionItem = await _repository.GetByIdAsync(id);
        return optionItem == null ? null : MapToDto(optionItem);
    }

    public async Task<OptionItemDto> CreateAsync(int optionGroupId, CreateOptionItemRequest request)
    {
        if (!await _optionGroupRepository.ExistsAsync(optionGroupId))
        {
            throw new ArgumentException($"OptionGroup with Id {optionGroupId} does not exist");
        }

        var optionItem = new OptionItem
        {
            OptionGroupId = optionGroupId,
            Name = request.Name,
            PriceAdjustment = request.PriceAdjustment,
            IsDefault = request.IsDefault,
            DisplayOrder = request.DisplayOrder
        };

        var created = await _repository.CreateAsync(optionItem);
        _logger.LogInformation("Created OptionItem: {Name} for OptionGroup {GroupId}", created.Name, optionGroupId);
        
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, CreateOptionItemRequest request)
    {
        var optionItem = await _repository.GetByIdAsync(id);
        if (optionItem == null)
        {
            _logger.LogWarning("OptionItem {Id} not found for update", id);
            return false;
        }

        optionItem.Name = request.Name;
        optionItem.PriceAdjustment = request.PriceAdjustment;
        optionItem.IsDefault = request.IsDefault;
        optionItem.DisplayOrder = request.DisplayOrder;

        var success = await _repository.UpdateAsync(optionItem);
        if (success)
        {
            _logger.LogInformation("Updated OptionItem: {Id} - {Name}", id, optionItem.Name);
        }
        
        return success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (success)
        {
            _logger.LogInformation("Deleted OptionItem: {Id}", id);
        }
        else
        {
            _logger.LogWarning("OptionItem {Id} not found for deletion", id);
        }
        
        return success;
    }

    private static OptionItemDto MapToDto(OptionItem optionItem)
    {
        return new OptionItemDto
        {
            Id = optionItem.Id,
            Name = optionItem.Name,
            PriceAdjustment = optionItem.PriceAdjustment,
            IsDefault = optionItem.IsDefault,
            DisplayOrder = optionItem.DisplayOrder
        };
    }
}
