using CoffeeShopApi.DTOs;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopApi.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IFileUploadService _fileUploadService;
    private readonly IProductRequestService _requestService;

    public ProductsController(
        IProductService service, 
        IFileUploadService fileUploadService,
        IProductRequestService requestService)
    {
        _service = service;
        _fileUploadService = fileUploadService;
        _requestService = requestService;
    }


    [HttpGet("Paged")]
    public async Task<IActionResult> GetAllPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] string? filter = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, search, orderBy, filter);
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(result));
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ProductFormDataRequest form)
    {
        try
        {
            var request = _requestService.ParseFormData<CreateProductRequest>(form.FormField);

            var validationErrors = _requestService.ValidateProductRequest(request);
            if (validationErrors.Any())
                return BadRequest(ApiResponse<object>.Fail(validationErrors));

            if (form.Image != null)
            {
                if (!_fileUploadService.ValidateImage(form.Image, out string imageError))
                {
                    return BadRequest(ApiResponse<object>.Fail(imageError));
                }

                request.ImageUrl = await _fileUploadService.UploadImageAsync(form.Image);
            }

            var created = await _service.CreateAsync(request);
            return Ok(ApiResponse<object>.Ok(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"File upload error: {ex.Message}"));
        }
    }


    [HttpPut("{id}")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Update(int id, [FromForm] ProductFormDataRequest form)
    {
        try
        {
            var request = _requestService.ParseFormData<UpdateProductRequest>(form.FormField);

            var validationErrors = _requestService.ValidateProductRequest(request);
            if (validationErrors.Any())
                return BadRequest(ApiResponse<object>.Fail(validationErrors));

            if (form.Image != null)
            {
                if (!_fileUploadService.ValidateImage(form.Image, out string imageError))
                {
                    return BadRequest(ApiResponse<object>.Fail(imageError));
                }

                request.ImageUrl = await _fileUploadService.UploadImageAsync(form.Image);
            }

            var success = await _service.UpdateAsync(id, request);
            if (!success) return NotFound(ApiResponse<object>.NotFound());

            return Ok(ApiResponse<object>.Ok(success, "Cập nhật sản phẩm thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"File upload error: {ex.Message}"));
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse<object>.NotFound());
        return Ok(ApiResponse<object>.Ok(success, "Xóa sản phẩm thành công"));
    }
}
