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
);


public record PaymentMethodCreateDto(
    Guid UserId,
    string CardNumber,
    string Brand,
    int ExpMonth,
    int ExpYear,
    string CardHolderName,
    bool IsDefault = false
);
