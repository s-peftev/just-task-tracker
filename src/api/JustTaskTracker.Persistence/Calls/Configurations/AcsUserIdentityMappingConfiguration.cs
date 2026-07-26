using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class AcsUserIdentityMappingConfiguration : IEntityTypeConfiguration<AcsUserIdentityMapping>
{
    public void Configure(EntityTypeBuilder<AcsUserIdentityMapping> builder)
    {
        builder.HasKey(m => m.UserId);
        builder.Property(m => m.AcsCommunicationUserId).IsRequired();
    }
}
