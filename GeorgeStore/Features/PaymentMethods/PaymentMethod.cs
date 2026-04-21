using GeorgeStore.Common;
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

    public static Result<PaymentMethod> Create(PaymentMethodCreateDto Dto)
    {
        if (Dto.CardNumber.Length != 16)
            return Result.Failure<PaymentMethod>(PaymentMethodError.InvalidCardNumber);

        if(Dto.ExpYear > 2099 && Dto.ExpYear < DateTime.Now.Year - 1)
            return Result.Failure<PaymentMethod>(PaymentMethodError.InvalidExpYear);

        var method = new PaymentMethod()
        {
            UserId = Dto.UserId,
            LastDigits = Dto.CardNumber[^4..],
            Brand = Dto.Brand,
            ExpMonth = Dto.ExpMonth,
            ExpYear = Dto.ExpYear,
            CardHolderName = Dto.CardHolderName,
            IsDefault = Dto.IsDefault,
            CreatedAt = DateTime.UtcNow,
            Token = Guid.NewGuid().ToString(),
        };

        return Result.Success(method);
    }
}

