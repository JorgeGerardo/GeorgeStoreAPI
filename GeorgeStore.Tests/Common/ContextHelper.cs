using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Data;

namespace GeorgeStore.Tests.Common;

public static class ContextHelper
{
    public static GeorgeStoreContext Create()
    {
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

    public static IDbConnection CreateSqlConn(GeorgeStoreContext context)
    {
        return context.Database.GetDbConnection();
    }

    public static Mock<IDbConnectionFactory> CreateConnectionFactory(IDbConnection sqlConn)
    {
        var connFactory = new Mock<IDbConnectionFactory>();
        connFactory.Setup(c => c.CreateConnection()).Returns(sqlConn);
        return connFactory;
    }

}
