namespace ElectroShopAPI.DTOs;

public record CategoryResponseDto(Guid Id, string Name);
public record BrandResponseDto(Guid Id, string Name);

public record ProductResponseDto(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    CategoryResponseDto? Category,
    BrandResponseDto? Brand
);