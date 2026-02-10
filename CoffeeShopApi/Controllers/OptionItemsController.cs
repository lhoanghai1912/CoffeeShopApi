using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OptionItemsController : ControllerBase
{
    private readonly IOptionItemService _service;
    private readonly ILogger<OptionItemsController> _logger;

    public OptionItemsController(
        IOptionItemService service,
        ILogger<OptionItemsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all option items
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Get option items by option group ID
    /// </summary>
    [HttpGet("group/{optionGroupId}")]
    public async Task<IActionResult> GetByOptionGroupId(int optionGroupId)
    {
        var result = await _service.GetByOptionGroupIdAsync(optionGroupId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Get option item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.NotFound($"OptionItem with Id {id} not found"));
        }
        
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Create a new option item for an option group
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/OptionItems/group/1
    ///     {
    ///       "name": "Size XL",
    ///       "priceAdjustment": 15000,
    ///       "isDefault": false,
    ///       "displayOrder": 4
    ///     }
    /// </remarks>
    [HttpPost("group/{optionGroupId}")]
    [Authorize(Policy = "RequirePermission:product.create")]
    public async Task<IActionResult> Create(int optionGroupId, [FromBody] CreateOptionItemRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<object>.Fail("Option item name is required"));
            }

            var created = await _service.CreateAsync(optionGroupId, request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<object>.Ok(created, "Option item created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating option item for group {GroupId}", optionGroupId);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating option item"));
        }
    }

    /// <summary>
    /// Update an existing option item
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequirePermission:product.update")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateOptionItemRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<object>.Fail("Option item name is required"));
            }

            var success = await _service.UpdateAsync(id, request);
            if (!success)
            {
                return NotFound(ApiResponse<object>.NotFound($"OptionItem with Id {id} not found"));
            }
            
            return Ok(ApiResponse<object>.Ok(success, "Option item updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating option item {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while updating option item"));
        }
    }

    /// <summary>
    /// Delete an option item
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequirePermission:product.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.NotFound($"OptionItem with Id {id} not found"));
            }
            
            return Ok(ApiResponse<object>.Ok(success, "Option item deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting option item {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while deleting option item"));
        }
    }
}
