using GeorgeStore.Common;

namespace GeorgeStore.Features.Orders;

public interface IOrderService
{
    Task<Result> Buy(Guid UserId, int CartId, int AddressId);
    Task<PagedResult<OrderDto>> Get(Guid UserId, QueryParams Prms);
    Task<Result<OrderDto>> GetById(Guid UserId, int OrderId);
}


