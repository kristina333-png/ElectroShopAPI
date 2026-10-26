using System.ComponentModel.DataAnnotations;

namespace ElectroShopAPI.DTOs;

public record CreateCategoryDto(
    [Required(ErrorMessage = "Название категории обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
    string Name
);

public record CreateBrandDto(
    [Required(ErrorMessage = "Название бренда обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
    string Name
);