using ElectroShopAPI.Data;
using ElectroShopAPI.DTOs;
using ElectroShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroShopAPI.Services;

public class ProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<List<ProductResponseDto>> GetFilteredAsync(
        string? search, Guid? categoryId, Guid? brandId,
        decimal? minPrice, decimal? maxPrice,
        int page, int pageSize)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);
        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId);
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        return await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto(
                p.Id,
                p.Name,
                p.Price,
                p.StockQuantity,
                p.Category != null ? new CategoryResponseDto(p.Category.Id, p.Category.Name) : null,
                p.Brand != null ? new BrandResponseDto(p.Brand.Id, p.Brand.Name) : null
            ))
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return null;

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.BrandId = dto.BrandId;
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return false;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }
}