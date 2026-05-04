using GeorgeStore.Common.Core;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Users;

namespace GeorgeStore.Features.PaymentMethods;

public class PaymentMethod : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string LastDigits { get; set; } = default!;
    public string Brand { get; set; } = default!;
    public int ExpMonth { get; set; }
    public int ExpYear { get; set; }
    public string CardHolderName { get; set; } = default!;
    public bool IsDefault { get; set; }
    public string Token { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public static Result<PaymentMethod> Create( Guid UserId, string CardNumber, string Brand, int ExpMonth, int ExpYear, string CardHolderName, bool IsDefault = false)
    {
        if (CardNumber.Length != 16)
            return Result.Failure<PaymentMethod>(PaymentMethodError.InvalidCardNumber);

        if(ExpYear > 2099 || ExpYear < DateTime.UtcNow.Year - 1)
            return Result.Failure<PaymentMethod>(PaymentMethodError.InvalidExpYear);

        var method = new PaymentMethod()
        {
            UserId = UserId,
            LastDigits = CardNumber[^4..],
            Brand = Brand,
            ExpMonth = ExpMonth,
            ExpYear = ExpYear,
            CardHolderName = CardHolderName,
            IsDefault = IsDefault,
            CreatedAt = DateTime.UtcNow,
            Token = Guid.NewGuid().ToString(),
        };

        return Result.Success(method);
    }
}

