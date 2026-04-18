using GeorgeStore.Common;

namespace GeorgeStore.Features.Carts;

public interface ICartRepository
{
    Task<Result<Cart>> GetAsync(Guid UserId, CancellationToken ct = default);
    Task<Result> AddAsync(Guid UserId, int ProductId, uint Quantity, CancellationToken ct = default);
    Task<Result> RemoveAsync(Guid UserId, int ProductId, CancellationToken ct = default);
    Task<int> CountAsync(Guid UserId);
}
