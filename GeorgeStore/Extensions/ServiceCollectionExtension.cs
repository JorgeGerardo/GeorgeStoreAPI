using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDBConnection(this IServiceCollection collection, WebApplicationBuilder builder)
    {
        collection.AddDbContext<GeorgeStoreContext>(opts =>
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
            .AddEntityFrameworkStores<GeorgeStoreContext>()
            .AddDefaultTokenProviders();

        return collection;
    }

    public static IServiceCollection AddDependencies(this IServiceCollection collection)
    {

        collection.AddSingleton<KeyedAsyncLock>();
        collection.AddScoped<ICategoryRepository, CategoryRepository>();
        collection.AddScoped<IUserService, UserService>();
        collection.AddScoped<ICartRepository, CartRepository>();
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        collection.AddScoped<TokenService>();


        return collection;
    }
}
