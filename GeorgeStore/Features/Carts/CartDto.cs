namespace GeorgeStore.Features.Carts;


public record CartItemDto(int Id, string Name, float Price, uint Quantity, string Description, string Image);
public record CartDto(List<CartItemDto> Items, float Total);
public record AddItemRequest(int ProductId, uint Quantity);

