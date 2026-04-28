using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.PaymentMethods;

public class PaymentMethodRepository(GeorgeStoreContext context, IDbConnectionFactory connection) : IPaymentMethodRepository
{
    public async Task<Result> Add(Guid UserId, PaymentMethodCreateDto Dto)
    {
        var result = PaymentMethod.Create(UserId, Dto.CardNumber, Dto.Brand, Dto.ExpMonth, Dto.ExpYear, Dto.CardHolderName, Dto.IsDefault);
        if (Dto.IsDefault)
        {
            var userPaymentMethods = await context.PaymentMethods.Where(p => p.UserId == UserId).ToListAsync();
            userPaymentMethods.ForEach(p => p.IsDefault = false);
        }

        if (!result.IsSuccess)
            return Result.Failure(result.Error);

        var methodsRegister = await context.PaymentMethods.CountAsync(p => p.UserId == UserId);
        if (methodsRegister >= 10)
            return Result.Failure(PaymentMethodError.PaymentMethodLimitReached);

        context.PaymentMethods.Add(result.Value);
        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(PaymentMethodError.UnexpectedError);
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAsync(Guid UserId)
    {
        var conn = connection.CreateConnection();
        const string query = """
            SELECT Id, UserId, LastDigits, Brand, ExpMonth, ExpYear, CardHolderName, IsDefault, CreatedAt FROM PaymentMethods
                WHERE UserId = @UserId
            """;

        return await conn.QueryAsync<PaymentMethodDto>(query, new { UserId });
    }

    public async Task<Result<PaymentMethod>> GetByIdAsync(Guid UserId, int Id)
    {
        PaymentMethod? paymentMethod = await context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == UserId && pm.Id == Id);

        return paymentMethod is not null
            ? Result.Success(paymentMethod)
            : Result.Failure<PaymentMethod>(PaymentMethodError.NotFound);
    }

    public async Task<Result> Remove(Guid UserId, int PaymentMethodId)
    {
        PaymentMethod? method = await context.PaymentMethods
            .FirstOrDefaultAsync(p => p.UserId == UserId && p.Id == PaymentMethodId);

        if (method is null)
            return Result.Failure(PaymentMethodError.NotFound);

        context.PaymentMethods.Remove(method);
        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(PaymentMethodError.UnexpectedError);
    }

    public async Task<Result> SetAsDefault(Guid UserId, int PaymentMethodId)
    {
        var userPaymentMethods = await context.PaymentMethods.Where(p => p.UserId == UserId).ToListAsync();
        if (!userPaymentMethods.Any(p => p.Id == PaymentMethodId))
            return Result.Failure(PaymentMethodError.NotFound);

        userPaymentMethods.ForEach(p => p.IsDefault = p.Id == PaymentMethodId);

        return await context.SaveChangesAsync() > 0 
            ? Result.Success() 
            : Result.Failure(PaymentMethodError.UnexpectedError);
    }
}

