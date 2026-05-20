using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Addresses.Queries.GetAddresses;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Carts.Query.GetCartItemsCount;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Categories.Queries.GetCategories;
using GeorgeStore.Features.Orders;
using GeorgeStore.Features.Orders.Queries.Get;
using GeorgeStore.Features.Orders.Queries.GetById;
using GeorgeStore.Features.PasswordRecovery;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.PaymentMethods.Queries;
using GeorgeStore.Features.Products;
using GeorgeStore.Features.Products.Queries.GetProducts;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.CQRS;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Infrastructure.Email.Brevo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDBConnection(this IServiceCollection collection, WebApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;


        builder.Services.AddDbContext<AdminContext>(opts =>
        {
            opts.UseNpgsql(builder.Configuration
                .GetConnectionString("GeorgeStoreConnection"))
                .UseSnakeCaseNamingConvention();
        });

        collection.AddDbContext<GeorgeStoreContext>(opts =>
            opts.UseNpgsql(builder.Configuration
                .GetConnectionString("GeorgeStoreConnection"))
                .UseSnakeCaseNamingConvention()
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
        collection.AddScoped<IOrderService, OrderService>();
        collection.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        collection.AddScoped<IAddressRepository, AddressRepository>();
        collection.AddScoped<ICategoryRepository, CategoryRepository>();
        collection.AddScoped<IUserService, UserService>();
        collection.AddScoped<ICartRepository, CartRepository>();
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        collection.AddScoped<IEmailSender, BrevoService>();
        collection.AddScoped<RecoverPasswordService>();
        collection.AddScoped<TokenService>();
        collection.AddScoped<AuthService>();

        //CQRS:
        collection.AddScoped<IQueryDispatcher, QueryDispatcher>();

        collection.AddScoped<
            IQueryHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>,
            GetCategoriesHandler>();

        collection.AddScoped<
            IQueryHandler<GetAddressesQuery, IEnumerable<AddressDto>>,
            GetAddressesHandler>();

        collection.AddScoped<
            IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>,
            GetProductsHandler>();

        collection.AddScoped<
            IQueryHandler<GetPaymentMethodsQuery, IEnumerable<PaymentMethodDto>>,
            GetPaymentMethodsHandler>();

        collection.AddScoped<
            IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>,
            GetOrderByIdHandler>();

        collection.AddScoped<
            IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>,
            GetOrdersHandler>();

        collection.AddScoped<
            IQueryHandler<GetCartItemsCountQuery, int>,
            GetCartItemsCountHandler>();


        return collection;
    }

    public static IServiceCollection AddExternalHttpClients(this IServiceCollection collection, IConfiguration config)
    {
        collection.AddHttpClient<IEmailSender, BrevoService>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/v3/");
            client.DefaultRequestHeaders.Add("api-key", config["Brevo:ApiKey"]);
        });

        return collection;
    }
}
