using GeorgeStore.Common;

namespace GeorgeStore.Features.Products;

public static class ProductError
{
    public static readonly Error Notfound =
        new("Product not found", "Can't find product selected", "Product.Notfound", ErrorType.NotFound);

    public static readonly Error Conflict =
        new("Has occurred an error", "Try again in some times", "Product.Conflict", ErrorType.Conflict);

}