using JustTaskTracker.Domain.Auth.Constants;
using JustTaskTracker.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Auth.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(UserFieldLengths.MaxEmailLength);
        builder.Property(u => u.DisplayName).HasMaxLength(UserFieldLengths.MaxDisplayNameLength);

        builder.HasIndex(u => u.AzureAdObjectId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
