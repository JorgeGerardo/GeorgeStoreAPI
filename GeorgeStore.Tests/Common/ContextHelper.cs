using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Tests.Common;

public static class ContextHelper
{
    public static GeorgeStoreContext Create()
    {
        //var options = new DbContextOptionsBuilder<GeorgeStoreContext>()
        //    .UseInMemoryDatabase(Guid.NewGuid().ToString())
        //    .Options;

        //return new GeorgeStoreContext(options);

        //Sqlite
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<GeorgeStoreContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GeorgeStoreContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    public static User CreateUser(GeorgeStoreContext context)
    {
        User user = new($"{Guid.NewGuid()}_TestUser", $"{Guid.NewGuid()}@gmail.com");
        context.Add(user);
        context.SaveChanges();
        return user;
    }

}
