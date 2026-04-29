using GeorgeStore.Features.Products;

namespace GeorgeStore.Features.Orders;

public sealed class OrderDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateUtc { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderDetailDto> Details { get; set; } = [];


    //Ship-Address Snapshot
    public string Street { get; set; } = default!;
    public string Neighborhood { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string? ExternalNumber { get; set; }
    public string? InternalNumber { get; set; }
    public string? References { get; set; }

    //Payment Snapshot
    public string CardHolderName { get; set; } = default!;
    public string Last4 { get; set; } = default!;
    public string Brand { get; set; } = default!;

}

public record OrderDetailDto(int Id, int OrderId, int ProductId, decimal UnitPrice, decimal SubTotal, int Quantity, string Image, string Name)
{
    public static OrderDetailDto FromEntity(OrderDetail entity)
    {
        return new OrderDetailDto(
            entity.Id, 
            entity.OrderId, 
            entity.ProductId, 
            entity.UnitPrice, 
            entity.SubTotal, 
            entity.Quantity, 
            entity.Product.Image, 
            entity.Product.Name
        );
    }
}

public sealed record BuyRequest(int CartId, int AddressId, int PaymentMethodId);

public sealed record ReorderRequest(int OrderId, int AddressId, int PaymentMethodId);

public sealed record ReorderPreview(int OrderId, IEnumerable<OrderDetailDto> Items, decimal Total, IEnumerable<ProductDto> InvalidItems);