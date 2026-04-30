using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Addresses;

[Route("api/[controller]")]
[ApiController]
public class AddressController(IAddressRepository addressRepository) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDto>>> Get()
    {
        var addresses = await addressRepository.Get(UserId);
        return Ok(addresses);
    }

    [HttpPost]
    public async Task<ActionResult> Add(AddressCreateDto request)
    {
        Result result = await addressRepository.Add(UserId, request);
        return result.IsSuccess
            ? Ok()
            : BadRequest(ProblemDetailFactory.FromError(result.Error));
    }

    [HttpDelete("{AddressId:int}")]
    public async Task<ActionResult> Delete([FromRoute] int AddressId)
    {
        Result result = await addressRepository.Remove(UserId, AddressId);
        return result.IsSuccess
            ? Ok()
            : NotFound(ProblemDetailFactory.FromError(result.Error));
    }

    [HttpPut("{AddressId:int}")]
    public async Task<ActionResult> SetAsDefault(int AddressId)
    {
        var result = await addressRepository.SetAsDefault(UserId, AddressId);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }
}
