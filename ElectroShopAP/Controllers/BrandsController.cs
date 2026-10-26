using ElectroShopAPI.Data;
using ElectroShopAPI.DTOs;
using ElectroShopAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectroShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BrandsController(AppDbContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _db.Brands.ToListAsync());

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var brand = await _db.Brands.FindAsync(id);
        return brand == null ? NotFound() : Ok(brand);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var brand = new Brand { Name = dto.Name };
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, brand);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateBrandDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var brand = await _db.Brands.FindAsync(id);
        if (brand == null) return NotFound();

        brand.Name = dto.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var brand = await _db.Brands.FindAsync(id);
        if (brand == null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.BrandId == id))
            return BadRequest(new { error = "Нельзя удалить бренд, так как к нему привязаны товары." });

        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}