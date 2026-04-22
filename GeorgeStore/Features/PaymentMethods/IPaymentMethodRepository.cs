using GeorgeStore.Common;

namespace GeorgeStore.Features.PaymentMethods;

public interface IPaymentMethodRepository
{
    Task<Result> Add(Guid UserId, PaymentMethodCreateDto Dto);
    Task<Result> Remove(Guid UserId, int PaymentMethodId);
    Task<Result> SetAsDefault(Guid UserId, int PaymentMethodId);
    Task<IEnumerable<PaymentMethodDto>> GetAsync(Guid UserId);
}

