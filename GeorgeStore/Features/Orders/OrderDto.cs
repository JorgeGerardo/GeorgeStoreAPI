namespace GeorgeStore.Features.Orders;

public sealed class OrderDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateUtc { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderDetailDto> Details { get; set; } = [];
}

public record OrderDetailDto(int Id, int OrderId, int ProductId, decimal UnitPrice, decimal SubTotal, int Quantity, string Image, string Name);

public sealed record BuyRequest(int CartId, int AddressId);

