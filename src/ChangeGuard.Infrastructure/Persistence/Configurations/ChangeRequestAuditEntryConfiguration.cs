using ChangeGuard.Domain.ChangeRequests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangeGuard.Infrastructure.Persistence.Configurations;

internal sealed class ChangeRequestAuditEntryConfiguration
    : IEntityTypeConfiguration<ChangeRequestAuditEntry>
{
    public void Configure(
        EntityTypeBuilder<ChangeRequestAuditEntry> builder)
    {
        builder.ToTable("ChangeRequestAuditEntries");

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id)
            .ValueGeneratedNever();

        builder.Property(entry => entry.Action)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(entry => entry.Actor)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(entry => entry.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(entry => entry.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entry => entry.OccurredUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(entry => new
            {
                entry.ChangeRequestId,
                entry.OccurredUtc
            })
            .HasDatabaseName(
                "IX_ChangeRequestAuditEntries_Request_OccurredUtc");

        builder.HasOne<ChangeRequest>()
            .WithMany()
            .HasForeignKey(entry => entry.ChangeRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
