using ElectroShopAPI.Data;
using ElectroShopAPI.DTOs;
using ElectroShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroShopAPI.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db) => _db = db;

    public async Task<Order> CreateAsync(Guid userId, CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("Заказ не может быть пустым.");

        var order = new Order
        {
            UserId = userId,
            DeliveryAddress = dto.DeliveryAddress,
            Status = OrderStatus.New
        };

        decimal total = 0;
        foreach (var item in dto.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId)
                ?? throw new InvalidOperationException($"Товар {item.ProductId} не найден.");
            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Недостаточно товара '{product.Name}' на складе.");

            product.StockQuantity -= item.Quantity;
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                PriceAtPurchase = product.Price
            };
            order.Items.Add(orderItem);
            total += product.Price * item.Quantity;
        }

        order.TotalAmount = total;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> GetByUserAsync(Guid userId) =>
        await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<List<Order>> GetAllAsync() =>
        await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> UpdateStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return null;
        order.Status = newStatus;
        await _db.SaveChangesAsync();
        return order;
    }
}