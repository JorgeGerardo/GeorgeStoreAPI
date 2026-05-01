using GeorgeStore.Common;

namespace GeorgeStore.Features.Orders;

public class OrderError
{
    public static readonly Error Notfound =
        new("Order not found", "Can't find any order with this id", "Order.Notfound", ErrorType.NotFound);

    public static readonly Error CartNotNotfound =
        new("Cart not found", "Can't find any cart with this id", "Order.CartNotNotfound", ErrorType.NotFound);
    
    public static readonly Error AddressNotFound =
        new("Address not found", "Address selected not found", "Order.AddressNotFound", ErrorType.NotFound);

}