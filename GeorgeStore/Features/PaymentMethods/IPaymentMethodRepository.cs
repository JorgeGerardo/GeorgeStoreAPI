using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.PaymentMethods;

public interface IPaymentMethodRepository
{
    Task<Result> AddAsync(Guid UserId, PaymentMethodCreateDto Dto);
    Task<Result> RemoveAsync(Guid UserId, int PaymentMethodId);
    Task<Result> SetAsDefaultAsync(Guid UserId, int PaymentMethodId);
    Task<Result<PaymentMethod>> GetByIdAsync(Guid UserId, int Id);
}

