using System.Security.Claims;
using ElectroShopAPI.DTOs;
using ElectroShopAPI.Models;
using ElectroShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroShopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orders;
    public OrdersController(OrderService orders) => _orders = orders;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState); 
        try
        {
            var order = await _orders.CreateAsync(CurrentUserId, dto);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyOrders() =>
        Ok(await _orders.GetByUserAsync(CurrentUserId));

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> All() =>
        Ok(await _orders.GetAllAsync());

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState); 
        var updated = await _orders.UpdateStatusAsync(id, (OrderStatus)dto.Status);
        return updated == null ? NotFound() : Ok(updated);
    }
}