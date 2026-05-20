using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.Addresses.Queries.GetAddresses;

public sealed class GetAddressesHandler(IDbConnectionFactory dbConnection) : IQueryHandler<GetAddressesQuery, IEnumerable<AddressDto>>
{
    public async Task<IEnumerable<AddressDto>> Handle(GetAddressesQuery query)
    {
        var connection = dbConnection.CreateConnection();
        const string sql = """
             SELECT
                 id, alias, street, neighborhood, city, state, postal_code, external_number, internal_number, "references", is_default
             FROM addresses
                WHERE user_id = @UserId
         """;

        return await connection.QueryAsync<AddressDto>(sql, new { query.UserId });
    }
}
