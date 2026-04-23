namespace GeorgeStore.Features.Orders;

public enum OrderStatus
{
    Pending,
    Paid,
    Processing,
    Completed,
    Canceled,
    PaymentFailed,
}