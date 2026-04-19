using GeorgeStore.Features.Addresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasData(
            new Address
            {
                Id = 1,
                Alias = "My principal house",
                UserId = new Guid("7aa8dc3a-4e4c-44d0-82cb-28f746d0c2c9"),
                City = "Ciudad de México",
                Neighborhood = "Roma",
                PostalCode = "99999",
                State = "México",
                Street = "Benito Juarez",
                ExternalNumber = "66",
                InternalNumber = "04",
                References = "Behind an Oxxo"
            }
        );
    }
}
