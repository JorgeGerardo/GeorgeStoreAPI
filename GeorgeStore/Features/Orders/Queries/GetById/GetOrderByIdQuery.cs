namespace GeorgeStore.Features.Orders.Queries.GetById;

public sealed record GetOrderByIdQuery(Guid UserId, int OrderId);
