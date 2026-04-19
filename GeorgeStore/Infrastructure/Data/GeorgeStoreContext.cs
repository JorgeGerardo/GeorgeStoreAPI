using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Infrastructure.Data;

public class GeorgeStoreContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public GeorgeStoreContext(DbContextOptions<GeorgeStoreContext> opts) : base(opts) { }


    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GeorgeStoreContext).Assembly);
    }

}
