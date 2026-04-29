namespace GeorgeStore.Features.Carts;


public sealed record CartItemDto(int Id, int ProductId, string Name, decimal Price, int Quantity, string Description, string Image)
{
    public static CartItemDto FromEntity(CartItem CartItem)
    {
        return new CartItemDto(
            CartItem.Id,
            CartItem.ProductId,
            CartItem.Item.Name,
            CartItem.Item.Price,
            CartItem.Quantity,
            CartItem.Item.Description,
            CartItem.Item.Image
        );
    }
}
public sealed record CartDto(int Id, List<CartItemDto> Items, decimal Total);
public sealed record AddItemRequest(int ProductId, int Quantity);

public sealed record DecreaseItemDto(int ProductId);