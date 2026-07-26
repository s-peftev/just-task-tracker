using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Common.Interfaces;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Application.Auth;

namespace JustTaskTracker.Persistence.Common;

public class JustTaskTrackerDbContext(
    DbContextOptions<JustTaskTrackerDbContext> options,
    ICurrentUserAccessor currentUserAccessor,
    IDateTimeProvider dateTimeProvider)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserGlobalRole> UserGlobalRoles => Set<UserGlobalRole>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<BoardTask> BoardTasks => Set<BoardTask>();
    public DbSet<BoardTaskComment> BoardTaskComments => Set<BoardTaskComment>();
    public DbSet<BoardTaskAttachment> BoardTaskAttachments => Set<BoardTaskAttachment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<StripeWebhookEvent> StripeWebhookEvents => Set<StripeWebhookEvent>();
    public DbSet<CallSession> CallSessions => Set<CallSession>();
    public DbSet<AcsUserIdentityMapping> AcsUserIdentityMappings => Set<AcsUserIdentityMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JustTaskTrackerDbContext).Assembly);

        BaseEntityConfiguration.Apply(modelBuilder);
        ConfigureSoftDeleteFilters(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateAuditProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        UpdateAuditProperties();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private void UpdateAuditProperties()
    {
        var currentUserId = TryResolveAuditUserId();
        var utcNow = dateTimeProvider.UtcNow;

        var changedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in changedEntries)
        {
            if (entry.Entity is ISoftDeletable deletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                deletable.IsDeleted = true;
                deletable.DeletedAtUtc = utcNow;
                deletable.DeletedBy = currentUserId;

                continue;
            }

            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAtUtc = utcNow;
                    auditable.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.LastModifiedAtUtc = utcNow;
                    auditable.LastModifiedBy = currentUserId;
                }
            }
        }
    }

    /// <summary>
    /// Anonymous/system contexts (e.g. Stripe webhooks) have no Entra user; audit columns stay null.
    /// </summary>
    private string? TryResolveAuditUserId()
    {
        try
        {
            return currentUserAccessor.AzureAdObjectId.ToString();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        var configureMethod = typeof(JustTaskTrackerDbContext)
            .GetMethod(nameof(ConfigureSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method {nameof(ConfigureSoftDeleteFilter)} was not found.");

        object[] parameters = [modelBuilder];

        var entityTypes = modelBuilder.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            if (entityType.BaseType == null && typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                configureMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(null, parameters);
            }
        }
    }

    private static void ConfigureSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
