using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OptionGroupsController : ControllerBase
{
    private readonly IOptionGroupService _service;
    private readonly ILogger<OptionGroupsController> _logger;

    public OptionGroupsController(
        IOptionGroupService service,
        ILogger<OptionGroupsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all option groups with their option items
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Get option group by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiResponse<object>.NotFound($"OptionGroup with Id {id} not found"));
        }
        
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Create a new option group with option items
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/OptionGroups
    ///     {
    ///       "name": "Kích cỡ",
    ///       "description": "Chọn kích cỡ đồ uống",
    ///       "isRequired": true,
    ///       "allowMultiple": false,
    ///       "displayOrder": 1,
    ///       "optionItems": [
    ///         {
    ///           "name": "Size S",
    ///           "priceAdjustment": 0,
    ///           "isDefault": true,
    ///           "displayOrder": 1
    ///         },
    ///         {
    ///           "name": "Size M",
    ///           "priceAdjustment": 5000,
    ///           "isDefault": false,
    ///           "displayOrder": 2
    ///         },
    ///         {
    ///           "name": "Size L",
    ///           "priceAdjustment": 10000,
    ///           "isDefault": false,
    ///           "displayOrder": 3
    ///         }
    ///       ]
    ///     }
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = "RequirePermission:product.create")]
    public async Task<IActionResult> Create([FromBody] CreateOptionGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<object>.Fail("Option group name is required"));
            }

            if (!request.OptionItems.Any())
            {
                return BadRequest(ApiResponse<object>.Fail("Option group must have at least one option item"));
            }

            var created = await _service.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<object>.Ok(created, "Option group created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating option group");
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating option group"));
        }
    }

    /// <summary>
    /// Update an existing option group (without items)
    /// </summary>
    /// <remarks>
    /// Note: To update option items, use the OptionItems endpoints
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Policy = "RequirePermission:product.update")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateOptionGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(ApiResponse<object>.Fail("Option group name is required"));
            }

            var success = await _service.UpdateAsync(id, request);
            if (!success)
            {
                return NotFound(ApiResponse<object>.NotFound($"OptionGroup with Id {id} not found"));
            }
            
            return Ok(ApiResponse<object>.Ok(success, "Option group updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating option group {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while updating option group"));
        }
    }

    /// <summary>
    /// Delete an option group
    /// </summary>
    /// <remarks>
    /// Warning: This will also delete all option items in this group
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequirePermission:product.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.NotFound($"OptionGroup with Id {id} not found"));
            }
            
            return Ok(ApiResponse<object>.Ok(success, "Option group deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting option group {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("An error occurred while deleting option group"));
        }
    }
}
