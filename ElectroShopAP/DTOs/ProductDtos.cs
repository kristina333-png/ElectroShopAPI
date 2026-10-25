using System.ComponentModel.DataAnnotations;

namespace ElectroShopAPI.DTOs;

public record CreateProductDto(
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 200 символов")]
    string Name,

    [Range(0.01, 1000000, ErrorMessage = "Цена должна быть больше 0")]
    decimal Price,

    [Range(0, 10000, ErrorMessage = "Количество не может быть отрицательным")]
    int StockQuantity,

    [Required]
    Guid CategoryId,

    [Required]
    Guid BrandId
);

public record UpdateProductDto(
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, MinimumLength = 2)]
    string Name,

    [Range(0.01, 1000000)]
    decimal Price,

    [Range(0, 10000)]
    int StockQuantity,

    [Required]
    Guid CategoryId,

    [Required]
    Guid BrandId
);