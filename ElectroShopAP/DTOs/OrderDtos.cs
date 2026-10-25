namespace ElectroShopAPI.DTOs;

public record OrderItemDto(Guid ProductId, int Quantity);
public record CreateOrderDto(string DeliveryAddress, List<OrderItemDto> Items);
public record UpdateOrderStatusDto(int Status);