using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShopApi.Data;
using CoffeeShopApi.DTOs;
using CoffeeShopApi.Models;
using CoffeeShopApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace CoffeeShopApi.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<bool> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
    Task<PaginatedResponse<ProductResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly AppDbContext _context;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, AppDbContext context)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _context = context;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllWithDetailsAsync();
        return products.Select(MapToResponse);
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id);
        return product == null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {           
        // determine category id: support either CategoryId or nested Category { id }
        int? categoryId = request.CategoryId;
        var categoryProp = request.GetType().GetProperty("Category");
        if (categoryId == null && categoryProp != null)
        {
            var catObj = categoryProp.GetValue(request);
            if (catObj != null)
            {
                var idProp = catObj.GetType().GetProperty("Id");
                if (idProp != null)
                {
                    var idVal = idProp.GetValue(catObj);
                    if (idVal is int idInt) categoryId = idInt;
                }
            }
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            BasePrice = request.BasePrice
        };

        // If frontend provided CategoryId, validate it and set
        if (categoryId != null)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId.Value);
            if (category == null)
                throw new ArgumentException("Category not found");

            product.CategoryId = category.Id;
            product.Category = category;
        }

        await _productRepository.CreateWithOptionsAsync(product);

        // ⭐ Thêm ProductOptionGroup mappings
        if (request.OptionGroups != null && request.OptionGroups.Any())
        {
            foreach (var ogRequest in request.OptionGroups)
            {
                // Kiểm tra OptionGroup có tồn tại không
                var optionGroup = await _context.OptionGroups
                    .Include(og => og.OptionItems)
                    .FirstOrDefaultAsync(og => og.Id == ogRequest.OptionGroupId);

                if (optionGroup == null)
                    throw new ArgumentException($"OptionGroup with ID {ogRequest.OptionGroupId} not found");

                // Validate allowedItemIds nếu có
                if (ogRequest.AllowedItemIds != null && ogRequest.AllowedItemIds.Any())
                {
                    var invalidIds = ogRequest.AllowedItemIds
                        .Where(itemId => !optionGroup.OptionItems.Any(oi => oi.Id == itemId))
                        .ToList();

                    if (invalidIds.Any())
                        throw new ArgumentException(
                            $"OptionGroup {ogRequest.OptionGroupId} does not contain items with IDs: {string.Join(", ", invalidIds)}");
                }

                // Tạo mapping
                _context.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = ogRequest.OptionGroupId,
                    DisplayOrder = ogRequest.DisplayOrder,
                    AllowedItemIds = ogRequest.AllowedItemIds
                });
            }
            await _context.SaveChangesAsync();
        }

        // Reload product with related data for response
        var created = await _productRepository.GetByIdWithDetailsAsync(product.Id);

        if (created == null) throw new InvalidOperationException("Created product not found");

        return MapToResponse(created);
    }


    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id);

        if (product == null) return false;

        product.Name = request.Name;
        product.Description = request.Description;
        product.ImageUrl = request.ImageUrl;
        product.BasePrice = request.BasePrice;

        // determine category id (support nested Category object or CategoryId)
        int? updCategoryId = request.CategoryId;
        var categoryProp2 = request.GetType().GetProperty("Category");
        if (updCategoryId == null && categoryProp2 != null)
        {
            var catObj = categoryProp2.GetValue(request);
            if (catObj != null)
            {
                var idProp = catObj.GetType().GetProperty("Id");
                if (idProp != null)
                {
                    var idVal = idProp.GetValue(catObj);
                    if (idVal is int idInt) updCategoryId = idInt;
                }
            }
        }

        if (updCategoryId != null)
        {
            var category = await _categoryRepository.GetByIdAsync(updCategoryId.Value);
            if (category == null)
                throw new ArgumentException("Category not found");
            product.CategoryId = category.Id;
            product.Category = category;
        }

        // ⭐ Cập nhật ProductOptionGroup mappings
        if (request.OptionGroups != null)
        {
            // Xóa tất cả mappings cũ
            var existingMappings = await _context.ProductOptionGroups
                .Where(pog => pog.ProductId == id)
                .ToListAsync();
            _context.ProductOptionGroups.RemoveRange(existingMappings);

            // Thêm mappings mới
            foreach (var ogRequest in request.OptionGroups)
            {
                // Kiểm tra OptionGroup có tồn tại không
                var optionGroup = await _context.OptionGroups
                    .Include(og => og.OptionItems)
                    .FirstOrDefaultAsync(og => og.Id == ogRequest.OptionGroupId);

                if (optionGroup == null)
                    throw new ArgumentException($"OptionGroup with ID {ogRequest.OptionGroupId} not found");

                // Validate allowedItemIds nếu có
                if (ogRequest.AllowedItemIds != null && ogRequest.AllowedItemIds.Any())
                {
                    var invalidIds = ogRequest.AllowedItemIds
                        .Where(itemId => !optionGroup.OptionItems.Any(oi => oi.Id == itemId))
                        .ToList();

                    if (invalidIds.Any())
                        throw new ArgumentException(
                            $"OptionGroup {ogRequest.OptionGroupId} does not contain items with IDs: {string.Join(", ", invalidIds)}");
                }

                // Tạo mapping mới
                _context.ProductOptionGroups.Add(new ProductOptionGroup
                {
                    ProductId = product.Id,
                    OptionGroupId = ogRequest.OptionGroupId,
                    DisplayOrder = ogRequest.DisplayOrder,
                    AllowedItemIds = ogRequest.AllowedItemIds
                });
            }
        }

        return await _productRepository.UpdateWithOptionsAsync(product, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _productRepository.DeleteWithOptionsAsync(id);
    }



    public async Task<PaginatedResponse<ProductResponse>> GetPagedAsync(int page, int pageSize, string? search, string? orderBy, string? filter)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, search, filter, orderBy);
        var mapped = items.Select(MapToResponse).ToList();
        return new PaginatedResponse<ProductResponse>(mapped, totalCount, page, pageSize);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            BasePrice = product.BasePrice,
            Category = product.Category == null ? null : new CategoryResponse
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            },
            OptionGroups = product.ProductOptionGroups
                .OrderBy(pog => pog.DisplayOrder)
                .Where(pog => pog.OptionGroup != null)
                .Select(pog =>
                {
                    var allowedIds = pog.AllowedItemIds;
                    var items = pog.OptionGroup!.OptionItems.AsEnumerable();

                    // ⭐ Filter items theo AllowedItemIds nếu có
                    if (allowedIds != null && allowedIds.Any())
                    {
                        items = items.Where(oi => allowedIds.Contains(oi.Id));
                    }

                    return new OptionGroupDto
                    {
                        Id = pog.OptionGroup!.Id,
                        Name = pog.OptionGroup.Name,
                        Description = pog.OptionGroup.Description,
                        IsRequired = pog.OptionGroup.IsRequired,
                        AllowMultiple = pog.OptionGroup.AllowMultiple,
                        DisplayOrder = pog.DisplayOrder,
                        DependsOnOptionItemId = pog.OptionGroup.DependsOnOptionItemId,
                        OptionItems = items
                            .OrderBy(oi => oi.DisplayOrder)
                            .Select(oi => new OptionItemDto
                            {
                                Id = oi.Id,
                                Name = oi.Name,
                                PriceAdjustment = oi.PriceAdjustment,
                                IsDefault = oi.IsDefault,
                                DisplayOrder = oi.DisplayOrder
                            }).ToList()
                    };
                }).ToList()
        };
    }
}