using GeorgeStore.Extensions;
using GeorgeStore.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(p => p.Total)
            .HasPrecision(18, 2);


        builder.SeedFromJson("Order.json");

    }
}
