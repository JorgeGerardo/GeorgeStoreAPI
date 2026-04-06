using GeorgeStore.Data;
using GeorgeStore.Models;
using GeorgeStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDBConnection(this IServiceCollection collection, WebApplicationBuilder builder)
    {
        collection.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(builder.Configuration
                .GetConnectionString("GeorgeStoreConnection"))
        );

        return collection;
    }
    public static IServiceCollection AddIdentity(this IServiceCollection collection)
    {
        collection.AddIdentity<User, IdentityRole<Guid>>(options =>
        { //Just for example:
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return collection;
    }

    public static IServiceCollection AddDependencies(this IServiceCollection collection)
    {
        collection.AddScoped<IUserRepository, UserService>();
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        collection.AddScoped<TokenService>();


        return collection;
    }
}
