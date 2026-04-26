using GeorgeStore.Common;

namespace GeorgeStore.Features.Orders;

public class OrderError
{
    public static readonly Error Notfound =
        new("Order not found", "Can't find any order with this id", "Order.Notfound");
}


