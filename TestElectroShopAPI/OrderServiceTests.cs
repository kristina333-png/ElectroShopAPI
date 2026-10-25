using ElectroShopAPI.Data;
using ElectroShopAPI.DTOs;
using ElectroShopAPI.Models;
using ElectroShopAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectroShopAPI.Tests;

public class OrderServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateOrder_ReducesStock_And_CalculatesTotal()
    {
        // Arrange
        var db = CreateDb();
        var product = new Product { Name = "Phone", Price = 100m, StockQuantity = 5 };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var service = new OrderService(db);
        var user = new User { Email = "test@test.com", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Act
        var order = await service.CreateAsync(user.Id, new CreateOrderDto("Address", new List<OrderItemDto>
        {
            new(product.Id, 2)
        }));

        // Assert
        Assert.Equal(200m, order.TotalAmount);
        Assert.Equal(3, db.Products.First().StockQuantity);
    }

    [Fact]
    public async Task CreateOrder_ThrowsWhenNotEnoughStock()
    {
        var db = CreateDb();
        var product = new Product { Name = "TV", Price = 500m, StockQuantity = 1 };
        db.Products.Add(product);
        var user = new User { Email = "u@u.com", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(user.Id, new CreateOrderDto("A", new List<OrderItemDto>
            {
                new(product.Id, 5)
            })));
    }

    [Fact]
    public async Task UpdateStatus_ChangesOrderStatus()
    {
        var db = CreateDb();
        var user = new User { Email = "s@s.com", PasswordHash = "x" };
        db.Users.Add(user);
        var order = new Order { UserId = user.Id, DeliveryAddress = "A", Status = OrderStatus.New };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        var updated = await service.UpdateStatusAsync(order.Id, OrderStatus.Shipped);

        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Shipped, updated!.Status);
    }
}