
namespace GeorgeStore.Features.PaymentMethods;

public record PaymentMethodDto(
    int Id,
    Guid UserId,
    string LastDigits,
    string Brand,
    int ExpMonth,
    int ExpYear,
    string CardHolderName,
    bool IsDefault,
    DateTime CreatedAt
)
{
    public static PaymentMethodDto FromEntity(PaymentMethod entity)
        => new(entity.Id, entity.UserId, entity.LastDigits, entity.Brand, entity.ExpMonth, entity.ExpYear, entity.CardHolderName, entity.IsDefault, entity.CreatedAt);

}


public record PaymentMethodCreateDto(
    Guid UserId, //TODO: Remove that
    string CardNumber,
    string Brand,
    int ExpMonth,
    int ExpYear,
    string CardHolderName,
    bool IsDefault = false
);
