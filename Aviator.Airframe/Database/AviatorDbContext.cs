using Microsoft.EntityFrameworkCore;

namespace Aviator.Airframe.Database;

public class AviatorDbContext(DbContextOptions<AviatorDbContext> options) : DbContext(options)
{
    public DbSet<AirframeEntity> Airframes => Set<AirframeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AirframeEntity>(e =>
        {
            e.ToTable("airframes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).HasColumnType("timestamp with time zone");
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.AcarsRegistration);
            e.HasIndex(x => x.FrameType);
            e.HasIndex(x => new { x.AcarsRegistration, x.Timestamp });
        });
    }
}
