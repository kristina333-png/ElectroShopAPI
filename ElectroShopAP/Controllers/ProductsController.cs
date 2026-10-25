using ElectroShopAPI.DTOs;
using ElectroShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;
    public ProductsController(ProductService products) => _products = products;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        [FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] Guid? brandId,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _products.GetFilteredAsync(search, categoryId, brandId, minPrice, maxPrice, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateProductDto dto) =>
        Ok(await _products.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto dto)
    {
        var updated = await _products.UpdateAsync(id, dto);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _products.DeleteAsync(id) ? NoContent() : NotFound();
}