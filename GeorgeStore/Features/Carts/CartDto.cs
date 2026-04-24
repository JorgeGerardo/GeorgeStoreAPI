namespace GeorgeStore.Features.Carts;


public sealed record CartItemDto(int Id, int ProductId, string Name, float Price, uint Quantity, string Description, string Image);
public sealed record CartDto(List<CartItemDto> Items, float Total);
public sealed record AddItemRequest(int ProductId, uint Quantity);

public sealed record DecreaseItemDto(int ProductId);