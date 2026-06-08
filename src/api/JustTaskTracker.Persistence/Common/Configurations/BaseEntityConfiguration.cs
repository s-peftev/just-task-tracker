using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Common.Configurations;

internal static class BaseEntityConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!IsDerivedFromBaseEntity(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property<bool>(nameof(ISoftDeletable.IsDeleted))
                .HasDefaultValue(false);
        }
    }

    private static bool IsDerivedFromBaseEntity(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                return true;
        }

        return false;
    }
}
