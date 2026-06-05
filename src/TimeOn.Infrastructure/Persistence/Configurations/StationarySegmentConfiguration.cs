using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for <see cref="CustomerVisit"/>.
/// </summary>
public sealed class StationarySegmentConfiguration : IEntityTypeConfiguration<StationarySegment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StationarySegment> builder)
    {
        builder.ToTable("StationarySegments");
        builder.HasKey(visit => visit.Id);

        builder.Property(visit => visit.WorkSessionId).IsRequired();
        builder.Property(visit => visit.CustomerId);
        builder.Property(visit => visit.StartUtc).IsRequired();
        builder.Property(visit => visit.EndUtc).IsRequired();
        builder.Property(visit => visit.CenterLatitude).IsRequired();
        builder.Property(visit => visit.CenterLongitude).IsRequired();
        builder.Property(visit => visit.DistanceFromCustomerMeters)
            .HasPrecision(10, 2);
    }
}
