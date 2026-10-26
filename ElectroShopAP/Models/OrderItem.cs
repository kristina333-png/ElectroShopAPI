using System.Text.Json.Serialization;

namespace ElectroShopAPI.Models;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public Guid ProductId { get; set; }

    [JsonIgnore] 
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
}