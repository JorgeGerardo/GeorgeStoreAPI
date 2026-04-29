using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.PaymentMethods;

[Route("api/[controller]")]
[ApiController]
public class PaymentMethodController(IPaymentMethodRepository repository) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Get()
    {
        return Ok(await repository.GetAsync(UserId));
    }

    [HttpGet("{PaymentMethodId:int}")]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Get([FromRoute] int PaymentMethodId)
    {
        var result = await repository.GetByIdAsync(UserId, PaymentMethodId);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] PaymentMethodCreateDto request)
    {
        var result = await repository.Add(UserId, request);
        return result.IsSuccess
            ? Ok()
            : BadRequest(result.Error);
    }

    [HttpDelete("{paymentMethodId:int}")]
    public async Task<ActionResult> Remove([FromRoute] int paymentMethodId)
    {
        var result = await repository.Remove(UserId, paymentMethodId);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }

    [HttpPut("{paymentMethodId:int}")]
    public async Task<ActionResult> UpdateDefaultPaymentMethod(int paymentMethodId)
    {
        var result = await repository.SetAsDefault(UserId, paymentMethodId);
        return result.IsSuccess ? Ok() : Conflict(result.Error);
    }
}
