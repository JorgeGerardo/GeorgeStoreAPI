using GeorgeStore.Extensions;
using GeorgeStore.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.Property(u => u.UserName).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder.SeedFromJson("User.json");

    }
}
