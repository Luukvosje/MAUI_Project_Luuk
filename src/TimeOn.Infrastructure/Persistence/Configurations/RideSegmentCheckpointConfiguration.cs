using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;
using TimeOn.Infrastructure.Persistence.Extensions;

namespace TimeOn.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for local-only <see cref="Checkpoint"/> data on <see cref="RideSegment"/>.
/// </summary>
/// <remarks>
/// Applied only by <see cref="LocalDeviceDbContext"/> to keep checkpoints off the API database.
/// </remarks>
public static class RideSegmentCheckpointConfiguration
{
    /// <summary>
    /// Configures owned checkpoint storage for offline distance calculation.
    /// </summary>
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RideSegment>()
            .OwnsMany(
                segment => segment.Checkpoints,
                checkpoints =>
                {
                    checkpoints.ToTable("Checkpoints");
                    checkpoints.WithOwner();
                    checkpoints.HasKey("RideSegmentId", nameof(Checkpoint.RecordedAtUtc));

                    checkpoints.Property(checkpoint => checkpoint.RecordedAtUtc).IsRequired();
                    checkpoints.OwnsOne(checkpoint => checkpoint.Location, location =>
                    {
                        OwnedCoordinateConfiguration.Configure(location);
                    });
                });

        modelBuilder.Entity<RideSegment>()
            .Navigation(segment => segment.Checkpoints)
            .HasField("_checkpoints")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
