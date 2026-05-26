using JustTaskTracker.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Configurations.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "auth");

        builder.Property(r => r.Name).HasMaxLength(64);
        builder.Property(r => r.NormalizedName).HasMaxLength(64);
        builder.Property(r => r.Description).HasMaxLength(256);

        builder.HasIndex(r => r.NormalizedName).IsUnique();
    }
}
