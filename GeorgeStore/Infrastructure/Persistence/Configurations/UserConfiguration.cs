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

        builder.HasData(
            new User("jorguito", "Jorguito@gmail.com")
            {
                Id = new Guid("7AA8DC3A-4E4C-44D0-82CB-28F746D0C2C9"),
                DateRegister = new DateTime(2022, 5, 10),
                UserName = "jorguito",
                NormalizedUserName = "JORGUITO",
                Email = "Jorguito@gmail.com",
                NormalizedEmail = "JORGUITO@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAECBVaFHLxUqWrsE1hmC3YZXfZup3yoYjXpBZFGDHX4PjbcrmcNq4BZ7L0OYieHT5Sg==",
                SecurityStamp = "STATIC_SECURITY_STAMP",
                ConcurrencyStamp = "STATIC_CONCURRENCY_STAMP"
            }
        );

    }
}
