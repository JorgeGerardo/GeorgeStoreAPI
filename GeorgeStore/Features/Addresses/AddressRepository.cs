using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.Addresses;

public class AddressRepository(GeorgeStoreContext context, IDbConnectionFactory dbConnection) : IAddressRepository
{
    public async Task<IEnumerable<AddressDto>> Get(Guid UserId)
    {
        var connection = dbConnection.CreateConnection();
        string query = """
            SELECT Id, Alias, Street, Neighborhood, City, [State], PostalCode, ExternalNumber, InternalNumber, [References]
            FROM Addresses
            WHERE UserId = @UserId
         """;

        return  await connection.QueryAsync<AddressDto>(query, new { UserId });
    }

    public async Task<Result> Add(Guid UserId, AddressDto Dto)
    {
        int AddressesRegistered = await context.Addresses.CountAsync(a => a.UserId == UserId);
        if (AddressesRegistered >= AddressLimits.MaxAddressesPerUser)
            return Result.Failure(AddressError.LimitReached);

        context.Addresses.Add(new Address
        {
            Alias = Dto.Alias,
            UserId = UserId,
            Street = Dto.Street,
            City = Dto.City,
            Neighborhood = Dto.Neighborhood,
            State = Dto.State,
            PostalCode = Dto.PostalCode,
            ExternalNumber = Dto.ExternalNumber,
            InternalNumber = Dto.InternalNumber,
            References = Dto.References

        });

        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(AddressError.Conflict);
    }

    public async Task<Result> Remove(Guid UserId, int AddressId)
    {
        int rows = await context.Addresses
            .Where(a => a.UserId == UserId && a.Id == AddressId)
            .ExecuteDeleteAsync();

        return rows == 0
            ? Result.Failure(AddressError.NotFound)
            : Result.Success();
    }

}
