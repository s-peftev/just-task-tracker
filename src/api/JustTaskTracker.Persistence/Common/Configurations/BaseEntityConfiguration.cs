using JustTaskTracker.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Common.Configurations;

internal static class BaseEntityConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>(nameof(ISoftDeletable.IsDeleted))
                .HasDefaultValue(false);
        }
    }
}
