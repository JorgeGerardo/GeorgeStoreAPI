using GeorgeStore.Common;
using GeorgeStore.Features.Users;

namespace GeorgeStore.Features.Carts;

public class Cart : Entity
{
    public User User { get; set; } = default!;
    public required Guid UserId { get; set; }
    public CartStatus Status { get; set; } = 0;
    public List<CartItem> Items { get; set; } = [];
    public float Total => Items.Sum(x => x.Item.Price * x.Quantity);

    public static Cart Create(Guid UserId)
    {
        return new Cart
        {
            UserId = UserId,
            Items = [],
            Status = CartStatus.Draft,
        };
    }
}
