using GeorgeStore.Common;
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
        return result.IsSuccess 
            ? Ok(PaymentMethodDto.FromEntity(result.Value)) 
            : HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] PaymentMethodCreateDto request)
    {
        var result = await repository.Add(UserId, request);
        return result.IsSuccess
            ? Ok()
            : HandleResult(result);
    }

    [HttpDelete("{paymentMethodId:int}")]
    public async Task<ActionResult> Remove([FromRoute] int paymentMethodId)
    {
        var result = await repository.Remove(UserId, paymentMethodId);
        return result.IsSuccess ? Ok() : HandleResult(result);
    }

    [HttpPut("{paymentMethodId:int}")]
    public async Task<ActionResult> UpdateDefaultPaymentMethod(int paymentMethodId)
    {
        var result = await repository.SetAsDefault(UserId, paymentMethodId);
        return result.IsSuccess ? Ok() : HandleResult(result);
    }
}
