using GeorgeStore.Extensions;
using GeorgeStore.Features.PaymentMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.Property(p => p.LastDigits).HasMaxLength(4);
        builder.Property(p => p.ExpMonth).HasMaxLength(2);
        builder.Property(p => p.ExpYear).HasMaxLength(4);

        builder.SeedFromJson("PaymentMethod.json");
    }
}
