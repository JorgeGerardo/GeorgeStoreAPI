namespace GeorgeStore.Features.Carts;

public enum CartStatus
{
    Draft = 0,
    Pending,
    Paid,       
    Processing,
    Completed,
    Canceled,
    PaymentFailed
}
