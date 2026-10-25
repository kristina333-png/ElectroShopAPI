using ElectroShopAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectroShopAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальный email
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Цены
        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("numeric(10,2)");
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("numeric(10,2)");
        modelBuilder.Entity<OrderItem>().Property(oi => oi.PriceAtPurchase).HasColumnType("numeric(10,2)");

        // Каскадное удаление позиций при удалении заказа (композиция)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> OrderItem: запрет каскада (чтобы не удалять товары)
        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}