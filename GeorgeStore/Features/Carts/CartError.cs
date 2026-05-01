using GeorgeStore.Common;

namespace GeorgeStore.Features.Carts;

public static class CartError
{
    public static readonly Error Notfound =
        new("Cart not found", "Try again in some minutes", "Cart.Notfound", ErrorType.NotFound);
    public static readonly Error ProductNotfound =
        new("Item cart not found", "Thise product dosen't exist in the cart", "Cart.ProductNotfound", ErrorType.NotFound);

    public static readonly Error ItemNotfound =
        new("Item not found", "Item not found in cart", "Cart.ItemNotfound", ErrorType.NotFound);

    public static readonly Error DecreaseLimit =
        new("Can't decrease current quantity because is 1", "If you want to remove the product of cart, you can remove", "Cart.DecreaseLimit", ErrorType.Validation);

}