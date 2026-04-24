using GeorgeStore.Common;
using GeorgeStore.Features.Products;

namespace GeorgeStore.Features.Carts;

public class CartItem : Entity
{
    public Cart Cart { get; set; } = default!;
    public int CartId { get; set; }
    public Product Item { get; set; } = default!;
    public required int ProductId { get; set; }
    public required uint Quantity { get; set; }

    public CartItemDto ToDto()
    {
        return new CartItemDto(
            Id, 
            ProductId,
            Item.Name, 
            Item.Price, 
            Quantity, 
            Item.Description, 
            Item.Image
        );
    }

}
