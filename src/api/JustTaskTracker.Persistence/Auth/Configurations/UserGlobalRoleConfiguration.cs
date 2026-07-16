using JustTaskTracker.Domain.Auth.Constants;
using JustTaskTracker.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Auth.Configurations;

public class UserGlobalRoleConfiguration : IEntityTypeConfiguration<UserGlobalRole>
{
    public void Configure(EntityTypeBuilder<UserGlobalRole> builder)
    {
        builder.HasKey(r => new { r.UserId, r.Role });

        builder.Property(r => r.Role)
            .HasMaxLength(UserFieldLengths.MaxGlobalRoleLength);

        builder.HasOne(r => r.User)
            .WithMany(u => u.GlobalRoles)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
