using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence.Configurations;

public sealed class WorkSessionConfiguration : IEntityTypeConfiguration<WorkSession>
{
    public void Configure(EntityTypeBuilder<WorkSession> builder)
    {
        builder.ToTable("WorkSessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.UserId).IsRequired();
        builder.Property(session => session.StartTimeUtc).IsRequired();
        builder.Property(session => session.EndTimeUtc);
        builder.Property(session => session.TotalDistanceKm).HasPrecision(12, 3);
        builder.Property(session => session.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasMany(session => session.RideSegments)
            .WithOne(segment => segment.WorkSession)
            .HasForeignKey(segment => segment.WorkSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(session => session.RideSegments)
            .HasField("_rideSegments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(session => session.CustomerVisits)
            .WithOne(visit => visit.WorkSession)
            .HasForeignKey(visit => visit.WorkSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(session => session.CustomerVisits)
            .HasField("_customerVisits")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
