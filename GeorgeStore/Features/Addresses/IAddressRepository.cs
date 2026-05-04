using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Addresses;

public interface IAddressRepository
{
    Task<IEnumerable<AddressDto>> GetAsync(Guid UserId);
    Task<Result> AddAsync(Guid UserId, AddressCreateDto Dto);
    Task<Result> RemoveAsync(Guid UserId, int AddressId);
    Task<Result> SetAsDefaultAsync(Guid UserId, int AddressId);
}
