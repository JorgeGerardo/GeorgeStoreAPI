using GeorgeStore.Common;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Users;

namespace GeorgeStore.Features.Orders;

public class Order : Entity
{
    public required Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public DateTime DateUtc { get; set; } = DateTime.UtcNow;
    public required decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderDetail> Details { get; set; } = [];


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

    public static Order Create(Guid UserId, decimal Total, Address Address, PaymentMethod Payment, IEnumerable<OrderDetail> Details)
    {
        return new Order
        {
            Total = Details.Sum(v => v.SubTotal),
            Details = Details.ToList(),
            UserId = UserId,

            Street = Address.Street,
            Neighborhood = Address.Neighborhood,
            City = Address.City,
            State = Address.State,
            PostalCode = Address.PostalCode,
            ExternalNumber = Address.ExternalNumber,
            InternalNumber = Address.InternalNumber,
            References = Address.References,

            CardHolderName = Payment.CardHolderName,
            Last4 = Payment.LastDigits,
            Brand = Payment.Brand,
        };

    }
}
