using GeorgeStore.Common;

namespace GeorgeStore.Features.Carts;

public static class CartError
{
    public static readonly Error Notfound =
        new("Cart not found", "Try again in some minutes", "Cart.Notfound");
    public static readonly Error ProductNotfound =
        new("Item cart not found", "Thise product dosen't exist in the cart", "Cart.ProductNotfound");

    public static readonly Error ItemNotfound =
        new("Item not found", "Item not found in cart", "Cart.ItemNotfound");

    public static readonly Error UnexpectedError =
        new("Unexpected error occurred", "Try again in some minutes", "Cart.UnexpectedError");

    public static readonly Error DecreaseLimit =
        new("Can't decrease current quantity because is 1", "If you want to remove the product of cart, you can remove", "Cart.DecreaseLimit");

}