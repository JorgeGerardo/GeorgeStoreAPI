using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Tests.Common;

public static class ContextHelper
{
    public static GeorgeStoreContext Create()
    {
        var options = new DbContextOptionsBuilder<GeorgeStoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GeorgeStoreContext(options);
    }

}
