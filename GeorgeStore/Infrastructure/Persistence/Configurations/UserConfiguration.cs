using GeorgeStore.Extensions;
using GeorgeStore.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.Property(u => u.UserName).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder.Property(x => x.DateRegister).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property(x => x.LockoutEnd).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.SeedFromJson("User.json");

    }
}
