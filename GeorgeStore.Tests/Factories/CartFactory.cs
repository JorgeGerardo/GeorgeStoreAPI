using GeorgeStore.Features.Carts;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Moq;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class CartFactory
{
    public static CartRepository CreateRepository(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = new Mock<IDbConnectionFactory>();
        connFactory.Setup(c => c.CreateConnection()).Returns(sqlConn);

        return new(context, connFactory.Object, new KeyedAsyncLock());
    }

}
