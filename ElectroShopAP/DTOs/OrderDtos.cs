using System.ComponentModel.DataAnnotations;

namespace ElectroShopAPI.DTOs;

public record OrderItemDto(
    [Required] Guid ProductId,
    [Range(1, 1000, ErrorMessage = "Количество должно быть от 1 до 1000")] int Quantity
);

public record CreateOrderDto(
    [Required(ErrorMessage = "Адрес доставки обязателен")]
    [StringLength(300, MinimumLength = 5)]
    string DeliveryAddress,

    [Required]
    [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы один товар")]
    List<OrderItemDto> Items
);

public record UpdateOrderStatusDto(
    [Range(0, 4, ErrorMessage = "Некорректный статус заказа")]
    int Status
);