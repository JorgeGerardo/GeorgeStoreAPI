using GeorgeStore.Features.Carts;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Tests.Factories;

internal static class CartFactory
{
    public static CartRepository CreateRepository(GeorgeStoreContext context)
    {
        return new(context, new KeyedAsyncLock());
    }

}
