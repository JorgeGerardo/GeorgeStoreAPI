using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Addresses;

[Route("api/[controller]")]
[ApiController]
public class AddressController(IAddressRepository addressRepository) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDto>>> Get()
    {
        var addresses = await addressRepository.GetAsync(UserId);
        return Ok(addresses);
    }

    [HttpPost]
    public async Task<ActionResult> Add(AddressCreateDto request)
    {
        Result result = await addressRepository.AddAsync(UserId, request);
        return HandleResult(result);
    }

    [HttpDelete("{AddressId:int}")]
    public async Task<ActionResult> Remove([FromRoute] int AddressId)
    {
        Result result = await addressRepository.RemoveAsync(UserId, AddressId);
        return HandleResult(result);
    }

    [HttpPut("{AddressId:int}")]
    public async Task<ActionResult> SetAsDefault(int AddressId)
    {
        var result = await addressRepository.SetAsDefaultAsync(UserId, AddressId);
        return HandleResult(result);
    }
}
