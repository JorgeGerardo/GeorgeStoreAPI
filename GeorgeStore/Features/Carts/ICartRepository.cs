using GeorgeStore.Common;

namespace GeorgeStore.Features.Carts;

public interface ICartRepository
{
    Task<Result<Cart>> GetAsync(Guid UserId, CancellationToken ct = default);
    Task<Result> AddAsync(Guid UserId, int ProductId, int Quantity, CancellationToken ct = default);
    Task<Result> DecreaseAsync(Guid UserId, int ProductId);
    Task<Result> RemoveAsync(Guid UserId, int ProductId, CancellationToken ct = default);
    Task<int> CountAsync(Guid UserId);
}
