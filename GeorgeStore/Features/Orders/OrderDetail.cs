using GeorgeStore.Common.Core;
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

    public static OrderDetail Create(int ProductId, decimal UnitPrice, decimal SubTotal, int Quantity)
    {
        return new OrderDetail
        {
            ProductId = ProductId,
            UnitPrice = UnitPrice,
            SubTotal = SubTotal,
            Quantity = Quantity
        };
    }
}


