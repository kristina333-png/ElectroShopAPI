using System.ComponentModel.DataAnnotations;

namespace ElectroShopAPI.DTOs;

public record RegisterDto(
    [Required]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    string Email,

    [Required]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    string Password
);

public record LoginDto(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password
);

public record AuthResponseDto(string Token, string Role);