using GeorgeStore.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.SubTotal)
            .HasPrecision(18, 2);

        builder.HasData(
            new OrderDetail()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 1_500,
                SubTotal = 3_000,
            },
            new OrderDetail()
            {
                Id = 2,
                OrderId = 1,
                ProductId = 2,
                Quantity = 5,
                UnitPrice = 800,
                SubTotal = 4_000,
            },

            new OrderDetail()
            {
                Id = 3,
                OrderId = 2,
                ProductId = 5,
                Quantity = 10,
                UnitPrice = 120,
                SubTotal = 1200,
            },
            new OrderDetail()
            {
                Id = 4,
                OrderId = 2,
                ProductId = 4,
                Quantity = 5,
                UnitPrice = 60,
                SubTotal = 600,
            },

            new OrderDetail()
            {
                Id = 5,
                OrderId = 3,
                ProductId = 3,
                Quantity = 1,
                UnitPrice = 25,
                SubTotal = 25,
            },
            new OrderDetail()
            {
                Id = 6,
                OrderId = 3,
                ProductId = 6,
                Quantity = 1,
                UnitPrice = 200,
                SubTotal = 200,
            },

            new OrderDetail()
            {
                Id = 7,
                OrderId = 4,
                ProductId = 1,
                Quantity = 1,
                UnitPrice = 1_500,
                SubTotal = 1_500,
            }
        );
    }
}
