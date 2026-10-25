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

// 4. Swagger с поддержкой JWT (Синтаксис для .NET 10)
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

    // Новый синтаксис для добавления требования безопасности
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// 5. Сервисы приложения (убедитесь, что папки называются так же)
builder.Services.AddScoped<ElectroShopAPI.Services.AuthService>();
builder.Services.AddScoped<ElectroShopAPI.Services.OrderService>();
builder.Services.AddScoped<ElectroShopAPI.Services.ProductService>();

var app = builder.Build();
// Сид администратора при первом запуске
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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