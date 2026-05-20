using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Features.PaymentMethods.Queries;
using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.PaymentMethods;

[Route("api/[controller]")]
[ApiController]
public class PaymentMethodController(IPaymentMethodRepository repository, IQueryDispatcher dispatcher) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Get()
    {
        var paymentMethods = await dispatcher.Send<GetPaymentMethodsQuery, IEnumerable<PaymentMethodDto>>(new(UserId));
        return Ok(paymentMethods);
    }

    [HttpGet("{PaymentMethodId:int}")]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetById([FromRoute] int PaymentMethodId)
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
    public async Task<ActionResult> UpdateDefaultAsync(int paymentMethodId)
    {
        var result = await repository.SetAsDefaultAsync(UserId, paymentMethodId);
        return HandleResult(result);
    }
}
