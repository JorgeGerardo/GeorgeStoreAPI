using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Orders;

public interface IOrderService
{
    Task<Result<int>> Purchase(Guid UserId, int CartId, int AddressId, int PaymentMethodId);
    Task<Result<int>> ReorderAsync(Guid UserId, ReorderRequest request);
    Task<Result<ReorderPreview>> PreviewReorder(Guid UserId, int OrderId);
    Task<PagedResult<OrderDto>> Get(Guid UserId, QueryParams Prms);
    Task<Result<OrderDto>> GetByIdAsync(Guid UserId, int OrderId);
}


