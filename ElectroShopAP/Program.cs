using System.Text;
using ElectroShopAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Контроллеры
builder.Services.AddControllers();

// 2. База данных PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Не найдена строка подключения DefaultConnection.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 3. JWT-аутентификация
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// 4. Swagger с поддержкой JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ElectroShopAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// 5. Сервисы приложения
builder.Services.AddScoped<ElectroShopAPI.Services.AuthService>();
builder.Services.AddScoped<ElectroShopAPI.Services.OrderService>();
builder.Services.AddScoped<ElectroShopAPI.Services.ProductService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // 1. Админ
    if (!db.Users.Any(u => u.Role == ElectroShopAPI.Models.UserRole.Admin))
    {
        db.Users.Add(new ElectroShopAPI.Models.User
        {
            Email = "admin@electroshop.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = ElectroShopAPI.Models.UserRole.Admin
        });
        db.SaveChanges();
    }

    // 2. Базовые данные (Категории, Бренды, Товары, Клиент, Заказы)
    if (!db.Categories.Any())
    {
        // Категории
        var catPhone = new ElectroShopAPI.Models.Category { Name = "Смартфоны" };
        var catLaptop = new ElectroShopAPI.Models.Category { Name = "Ноутбуки" };

        // Бренды
        var brandApple = new ElectroShopAPI.Models.Brand { Name = "Apple" };
        var brandSamsung = new ElectroShopAPI.Models.Brand { Name = "Samsung" };

        db.Categories.AddRange(catPhone, catLaptop);
        db.Brands.AddRange(brandApple, brandSamsung);
        db.SaveChanges();

        // 12 Товаров (основная сущность)
        var products = new List<ElectroShopAPI.Models.Product>();
        for (int i = 1; i <= 12; i++)
        {
            products.Add(new ElectroShopAPI.Models.Product
            {
                Name = $"Смартфон/Ноутбук Модель {i}",
                Price = 1000m * i,
                StockQuantity = 10,
                CategoryId = (i % 2 == 0) ? catPhone.Id : catLaptop.Id,
                BrandId = (i % 2 == 0) ? brandApple.Id : brandSamsung.Id
            });
        }
        db.Products.AddRange(products);
        db.SaveChanges();

        // Тестовый Клиент
        var client = new ElectroShopAPI.Models.User
        {
            Email = "client@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client123!"),
            Role = ElectroShopAPI.Models.UserRole.Client
        };
        db.Users.Add(client);
        db.SaveChanges();

        // 3 Заказа (связанная сущность)
        var order1 = new ElectroShopAPI.Models.Order { UserId = client.Id, DeliveryAddress = "ул. Пушкина, д. 1", TotalAmount = 3000m, Status = ElectroShopAPI.Models.OrderStatus.New };
        var order2 = new ElectroShopAPI.Models.Order { UserId = client.Id, DeliveryAddress = "ул. Лермонтова, д. 2", TotalAmount = 5000m, Status = ElectroShopAPI.Models.OrderStatus.Shipped };
        var order3 = new ElectroShopAPI.Models.Order { UserId = client.Id, DeliveryAddress = "ул. Гоголя, д. 3", TotalAmount = 2000m, Status = ElectroShopAPI.Models.OrderStatus.Delivered };

        db.Orders.AddRange(order1, order2, order3);
        db.SaveChanges();

        // Позиции заказов (OrderItems)
        db.OrderItems.AddRange(
            new ElectroShopAPI.Models.OrderItem { OrderId = order1.Id, ProductId = products[0].Id, Quantity = 1, PriceAtPurchase = products[0].Price },
            new ElectroShopAPI.Models.OrderItem { OrderId = order2.Id, ProductId = products[1].Id, Quantity = 2, PriceAtPurchase = products[1].Price },
            new ElectroShopAPI.Models.OrderItem { OrderId = order3.Id, ProductId = products[2].Id, Quantity = 1, PriceAtPurchase = products[2].Price }
        );
        db.SaveChanges();
    }
}

// 6. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }