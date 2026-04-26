using GeorgeStore.Extensions;
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

        builder.SeedFromJson("CartItem.json");
    }
}
