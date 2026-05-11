using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Carts;

public static class CartError
{
    public static readonly Error Notfound =
        new("Cart not found", "Car was not found, try again in some minutes", "Cart.Notfound", ErrorType.NotFound);

    public static readonly Error ProductNotfound =
        new("Item cart not found", "This product dosen't exist in the cart", "Cart.ProductNotfound", ErrorType.NotFound);

    public static readonly Error ItemNotfound =
        new("Item not found", "Item not found in the cart", "Cart.ItemNotfound", ErrorType.NotFound);

    public static readonly Error ItemNotAvailable =
        new("Item not available", "This item isn't available", "Cart.ItemNotAvailable", ErrorType.Validation);

    public static readonly Error DecreaseLimit =
        new("Can't decrease current quantity because is 1", "This product has the minimum quantity, if you want to remove, try press the trash button", "Cart.DecreaseLimit", ErrorType.Validation);

}