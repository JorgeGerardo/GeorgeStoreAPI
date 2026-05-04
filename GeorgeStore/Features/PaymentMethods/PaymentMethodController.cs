using GeorgeStore.Features.Shared.Base;
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
        var result = await repository.AddAsync(UserId, request);
        return HandleResult(result);
    }

    [HttpDelete("{paymentMethodId:int}")]
    public async Task<ActionResult> Remove([FromRoute] int paymentMethodId)
    {
        var result = await repository.RemoveAsync(UserId, paymentMethodId);
        return HandleResult(result);
    }

    [HttpPut("{paymentMethodId:int}")]
    public async Task<ActionResult> UpdateDefault(int paymentMethodId)
    {
        var result = await repository.SetAsDefaultAsync(UserId, paymentMethodId);
        return HandleResult(result);
    }
}
