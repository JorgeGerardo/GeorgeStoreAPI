using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(ProductQueryParams Prms);

public record ProductQueryParams(int? CategoryId) : QueryParams;