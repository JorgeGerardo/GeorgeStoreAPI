using GeorgeStore.Features.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder
        .HasIndex(ci => new { ci.CartId, ci.ProductId })
        .IsUnique();

        builder.HasData(
            new CartItem
            {
                Id = 1,
                CartId = 1,
                ProductId = 1,
                Quantity = 2,
            },
            new CartItem
            {
                Id = 2,
                CartId = 1,
                ProductId = 2,
                Quantity = 2,
            }
        );
    }
}
