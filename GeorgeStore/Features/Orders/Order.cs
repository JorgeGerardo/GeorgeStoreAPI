using GeorgeStore.Common;
using GeorgeStore.Features.Users;

namespace GeorgeStore.Features.Orders;

public class Order : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public DateTime DateUtc { get; set; } = DateTime.UtcNow;
    public required decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderDetail> Details { get; set; } = [];
}


