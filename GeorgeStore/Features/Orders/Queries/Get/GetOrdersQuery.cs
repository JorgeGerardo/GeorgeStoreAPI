using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Orders.Queries.Get;

public sealed record GetOrdersQuery(Guid UserId, QueryParams Prms);
