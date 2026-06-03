using JustTaskTracker.Domain.Kanban.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Kanban.Configurations;

public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
{
    public void Configure(EntityTypeBuilder<BoardMember> builder)
    {
        builder.HasKey(m => new { m.BoardId, m.UserId });

        builder.Property(m => m.Role)
            .HasConversion<byte>();

        builder.HasOne(m => m.Board)
            .WithMany(b => b.Members)
            .HasForeignKey(m => m.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(m => m.UserId);

        builder.HasIndex(m => m.BoardId)
            .IsUnique()
            .HasFilter("[Role] = 1");
    }
}
