using GeorgeStore.Common;

namespace GeorgeStore.Features.Addresses;

public interface IAddressRepository
{
    Task<IEnumerable<AddressDto>> Get(Guid UserId);
    Task<Result> Add(Guid UserId, AddressCreateDto Dto);
    Task<Result> Remove(Guid UserId, int AddressId);
}
