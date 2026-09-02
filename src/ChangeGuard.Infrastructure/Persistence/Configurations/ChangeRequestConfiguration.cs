using ChangeGuard.Domain.ChangeRequests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChangeGuard.Infrastructure.Persistence.Configurations;

internal sealed class ChangeRequestConfiguration
    : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(
        EntityTypeBuilder<ChangeRequest> builder)
    {
        builder.ToTable("ChangeRequests");

        builder.HasKey(changeRequest => changeRequest.Id);

        builder.Property(changeRequest => changeRequest.Id)
            .ValueGeneratedNever();

        builder.Property(changeRequest =>
                changeRequest.ReferenceNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(changeRequest =>
                changeRequest.ReferenceNumber)
            .IsUnique()
            .HasDatabaseName(
                "UX_ChangeRequests_ReferenceNumber");

        builder.Property(changeRequest => changeRequest.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(changeRequest => changeRequest.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.HasQaEvidence)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.QaEvidenceNotes)
            .HasMaxLength(4000);

        builder.Property(changeRequest =>
                changeRequest.HasRollbackPlan)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.RollbackPlan)
            .HasMaxLength(4000);

        builder.Property(changeRequest =>
                changeRequest.CreatedUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(changeRequest =>
                changeRequest.UpdatedUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();
    }
}
