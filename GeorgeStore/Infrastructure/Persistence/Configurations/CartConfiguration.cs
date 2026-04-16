using GeorgeStore.Features.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasData(
            new Cart
            {
                Id = 1,
                UserId = new Guid("7AA8DC3A-4E4C-44D0-82CB-28F746D0C2C9"),
            }
        );
    }
}
