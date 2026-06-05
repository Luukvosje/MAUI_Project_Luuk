using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence.Configurations;

public sealed class DrivingSegmentConfiguration : IEntityTypeConfiguration<DrivingSegment>
{
    public void Configure(EntityTypeBuilder<DrivingSegment> builder)
    {
        builder.ToTable("DrivingSegments");
        builder.HasKey(segment => segment.Id);
        builder.Property(segment => segment.WorkSessionId).IsRequired();
        builder.Property(segment => segment.StartUtc).IsRequired();
        builder.Property(segment => segment.EndUtc).IsRequired();
        builder.Property(segment => segment.DistanceMeters).IsRequired();
    }
}
