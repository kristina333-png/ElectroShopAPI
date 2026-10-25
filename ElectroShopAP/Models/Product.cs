namespace ElectroShopAPI.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}