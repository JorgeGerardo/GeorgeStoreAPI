using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Orders;

public static class OrderError
{
    public static readonly Error Notfound =
        new("Order not found", "Can't find any order with this id", "Order.Notfound", ErrorType.NotFound);

    public static readonly Error CartNotNotfound =
        new("Cart not found", "Can't find any cart with this id", "Order.CartNotNotfound", ErrorType.NotFound);
    
    public static readonly Error AddressNotFound =
        new("Address not found", "Address selected not found", "Order.AddressNotFound", ErrorType.NotFound);
    
    public static readonly Error PaymentMethodNotFound =
        new("Payment method not found", "Try with another", "Order.PaymentMethodNotFound", ErrorType.NotFound);

    public static readonly Error NoValidItems =
        new("Without valid products", "No one of these product are válid, try with another order or make a new purchase", "Order.NoValidItems", ErrorType.Validation);

}