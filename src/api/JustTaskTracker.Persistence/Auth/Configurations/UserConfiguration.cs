using JustTaskTracker.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Auth.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(320);
        builder.Property(u => u.DisplayName).HasMaxLength(256);

        builder.HasIndex(u => u.AzureAdObjectId)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");
    }
}
