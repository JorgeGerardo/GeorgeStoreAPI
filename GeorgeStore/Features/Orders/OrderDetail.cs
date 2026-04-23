using GeorgeStore.Common;
using GeorgeStore.Features.Products;

namespace GeorgeStore.Features.Orders;

public class OrderDetail : Entity
{
    public Order Order { get; set; } = default!;
    public int OrderId { get; set; }
    public Product Product { get; set; } = default!;
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int Quantity { get; set; }
}


