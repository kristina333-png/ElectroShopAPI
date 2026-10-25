namespace ElectroShopAPI.Models;

public enum UserRole
{
    Client = 0,
    Admin = 1
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}