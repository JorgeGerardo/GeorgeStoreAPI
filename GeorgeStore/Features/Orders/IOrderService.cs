using GeorgeStore.Common;
using GeorgeStore.Features.Carts;

namespace GeorgeStore.Features.Orders;

public interface IOrderService
{
    Task<Result> Buy(Cart cart);
    Task<IEnumerable<OrderDto>> Get(Guid UserId, QueryParams Prms);
    Task<Result<OrderDto>> GetById(Guid UserId, int OrderId);
}


