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

        return await connection.QueryAsync<AddressDto>(query, new { UserId });
    }

    public async Task<Result> Add(Guid UserId, AddressCreateDto Dto)
    {
        int AddressesRegistered = await context.Addresses.CountAsync(a => a.UserId == UserId);
        if (AddressesRegistered >= AddressLimits.MaxAddressesPerUser)
            return Result.Failure(AddressError.LimitReached);

        Address newAddress = Address.Create(UserId, Dto.Alias, Dto.Street, Dto.Neighborhood, Dto.City, Dto.State, Dto.PostalCode, Dto.ExternalNumber, Dto.InternalNumber, Dto.References);
        context.Addresses.Add(newAddress);

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
