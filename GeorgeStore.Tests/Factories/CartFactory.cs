using GeorgeStore.Features.Carts;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class CartFactory
{
    public static CartRepository CreateRepository(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = ContextHelper.CreateConnectionFactory(sqlConn);
        return new(context, connFactory.Object, new KeyedAsyncLock());
    }

}
