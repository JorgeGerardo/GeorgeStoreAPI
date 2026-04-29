using GeorgeStore.Common;

namespace GeorgeStore.Features.Orders;

public interface IOrderService
{
    Task<Result<int>> Buy(Guid UserId, int CartId, int AddressId, int PaymentMethodId);
    Task<Result<int>> ReorderAsync(Guid UserId, ReorderRequest request);
    Task<Result<ReorderPreview>> PreviewReorder(Guid UserId, int OrderId);
    Task<PagedResult<OrderDto>> Get(Guid UserId, QueryParams Prms);
    Task<Result<OrderDto>> GetById(Guid UserId, int OrderId);
}


