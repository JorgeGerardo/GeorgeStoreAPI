using GeorgeStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GeorgeStore.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }


    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>().ToTable("Users");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var categoryFile = File.ReadAllText(Path.Combine("Data", "DataSeed","Category.json"));
        var categories = JsonSerializer.Deserialize<List<Category>>(categoryFile, options: options);
        if (categories is null || !categories.Any())
            throw new Exception("Category Data isn't exist");
        builder.Entity<Category>().HasData(categories);


        var productFile = File.ReadAllText(Path.Combine("Data", "DataSeed", "Product.json"));
        var products = JsonSerializer.Deserialize<List<Product>>(productFile, options: options);
        if (products is null || !products.Any())
            throw new Exception("Product Data isn't exist");
        builder.Entity<Product>().HasData(products);

        builder.Entity<User>().HasData(
            new User
            {
                Id = new Guid("7AA8DC3A-4E4C-44D0-82CB-28F746D0C2C9"),
                UserName = "jorguito",
                NormalizedUserName = "JORGUITO",
                Email = "Jorguito@gmail.com",
                NormalizedEmail = "JORGUITO@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEMNCL4B+gPr4VcM82UiN7utLbcTxQDN0K6tAbSEQLjRwecDnQ+nWSTRMKl2aX5xA9w==",
                SecurityStamp = "STATIC_SECURITY_STAMP",
                ConcurrencyStamp = "STATIC_CONCURRENCY_STAMP"
            }
        );


    }

}
