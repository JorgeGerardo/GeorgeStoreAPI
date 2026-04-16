using GeorgeStore.Common;

namespace GeorgeStore.Features.Carts;

public static class CartError
{
    public static readonly Error Notfound =
        new("Cart not found", "Try again in some minutes", "Cart.Notfound");

    public static readonly Error ItemNotfound =
        new("Item not found", "Item not found in cart", "Cart.ItemNotfound");

    public static readonly Error UnexpectedError =
        new("Unexpected error occurred", "Try again in some minutes", "Cart.UnexpectedError");

}