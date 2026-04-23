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

        builder.HasData(
            new Order()
            {
                Id = 1,
                UserId = new Guid("7aa8dc3a-4e4c-44d0-82cb-28f746d0c2c9"),
                Total = 7_000,
                DateUtc = new DateTime(2026, 4, 22)
            },
            new Order()
            {
                Id = 2,
                UserId = new Guid("7aa8dc3a-4e4c-44d0-82cb-28f746d0c2c9"),
                Total = 1_800,
                DateUtc = new DateTime(2026, 4, 20)
            },
            new Order()
            {
                Id = 3,
                UserId = new Guid("7aa8dc3a-4e4c-44d0-82cb-28f746d0c2c9"),
                Total = 9_000,
                DateUtc = new DateTime(2026, 3, 12)
            },
            new Order()
            {
                Id = 4,
                UserId = new Guid("7aa8dc3a-4e4c-44d0-82cb-28f746d0c2c9"),
                Total = 9_000,
                DateUtc = new DateTime(2026, 1, 2)
            }
        );
    }
}
