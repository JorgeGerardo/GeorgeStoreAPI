using GeorgeStore.Common;
using GeorgeStore.Features.Products;

namespace GeorgeStore.Features.Carts;

public class CartItem : Entity
{
    public Cart Cart { get; set; } = default!;
    public int CartId { get; set; }
    public Product Item { get; set; } = default!;
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }


}
