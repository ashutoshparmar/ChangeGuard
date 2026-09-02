using ChangeGuard.Domain.ChangeRequests;

using Microsoft.EntityFrameworkCore;

namespace ChangeGuard.Infrastructure.Persistence;

public sealed class ChangeGuardDbContext : DbContext
{
    public ChangeGuardDbContext(
        DbContextOptions<ChangeGuardDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChangeRequest> ChangeRequests =>
        Set<ChangeRequest>();

    public DbSet<ChangeRequestAuditEntry> ChangeRequestAuditEntries =>
        Set<ChangeRequestAuditEntry>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ChangeGuardDbContext).Assembly);
    }
}
