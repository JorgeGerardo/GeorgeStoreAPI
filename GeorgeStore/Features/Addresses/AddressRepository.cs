using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.Addresses;

public class AddressRepository(GeorgeStoreContext context, IDbConnectionFactory dbConnection) : IAddressRepository
{
    public async Task<IEnumerable<AddressDto>> GetAsync(Guid UserId)
    {
        var connection = dbConnection.CreateConnection();
        const string query = """
            SELECT Id, Alias, Street, Neighborhood, City, [State], PostalCode, ExternalNumber, InternalNumber, [References], IsDefault
            FROM Addresses
            WHERE UserId = @UserId
         """;

        return await connection.QueryAsync<AddressDto>(query, new { UserId });
    }

    public async Task<Result> AddAsync(Guid UserId, AddressCreateDto request)
    {
        int AddressesRegistered = await context.Addresses.CountAsync(a => a.UserId == UserId);
        if (AddressesRegistered >= AddressLimits.MaxAddressesPerUser)
            return Result.Failure(AddressError.LimitReached);

        if (request.IsDefault)
        {
            var userAddress = await context.Addresses.Where(a => a.UserId == UserId).ToListAsync();
            userAddress.ForEach(a => a.IsDefault = false);
        }


        Address newAddress = Address.Create(UserId, request.Alias, request.Street, request.Neighborhood, request.City, request.State, request.PostalCode, request.ExternalNumber, request.InternalNumber, request.References, request.IsDefault);
        context.Addresses.Add(newAddress);

        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(Guid UserId, int AddressId)
    {
        int rows = await context.Addresses
            .Where(a => a.UserId == UserId && a.Id == AddressId)
            .ExecuteDeleteAsync();

        return rows == 0
            ? Result.Failure(AddressError.NotFound)
            : Result.Success();
    }

    public async Task<Result> SetAsDefaultAsync(Guid UserId, int AddressId)
    {
        var userAddress = await context.Addresses.Where(a => a.UserId == UserId).ToListAsync();
        if (!userAddress.Any(a => a.Id == AddressId))
            return Result.Failure(AddressError.NotFound);

        userAddress.ForEach(a => a.IsDefault = a.Id == AddressId);
        await context.SaveChangesAsync();
        return Result.Success();
        
    }
}
