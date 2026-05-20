using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.PaymentMethods.Queries;

public class GetPaymentMethodsHandler(IDbConnectionFactory _db) : IQueryHandler<GetPaymentMethodsQuery, IEnumerable<PaymentMethodDto>>
{
    public async Task<IEnumerable<PaymentMethodDto>> Handle(GetPaymentMethodsQuery query)
    {
        var conn = _db.CreateConnection();
        const string sql = """
            SELECT 
                id, user_id, last_digits, brand, exp_month, exp_year, card_holder_name, is_default, created_at 
            FROM payment_methods
            	WHERE user_id = @UserId
            """;

        return await conn.QueryAsync<PaymentMethodDto>(sql, new { query.UserId });
    }
}
