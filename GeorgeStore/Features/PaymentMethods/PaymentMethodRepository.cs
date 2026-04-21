using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.PaymentMethods;

public class PaymentMethodRepository(GeorgeStoreContext context, IDbConnectionFactory connection) : IPaymentMethodRepository
{
    public async Task<Result> Add(Guid UserId, PaymentMethodCreateDto Dto)
    {
        var result = PaymentMethod.Create(Dto);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);

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
}

