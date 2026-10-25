namespace ElectroShopAPI.DTOs;

public record CreateProductDto(string Name, decimal Price, int StockQuantity, Guid CategoryId, Guid BrandId);
public record UpdateProductDto(string Name, decimal Price, int StockQuantity, Guid CategoryId, Guid BrandId);