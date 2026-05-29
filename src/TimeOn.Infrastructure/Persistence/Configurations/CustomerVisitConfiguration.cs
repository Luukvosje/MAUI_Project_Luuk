using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for <see cref="CustomerVisit"/>.
/// </summary>
public sealed class CustomerVisitConfiguration : IEntityTypeConfiguration<CustomerVisit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomerVisit> builder)
    {
        builder.ToTable("CustomerVisits");
        builder.HasKey(visit => visit.Id);

        builder.Property(visit => visit.WorkSessionId).IsRequired();
        builder.Property(visit => visit.CustomerId).IsRequired();
        builder.Property(visit => visit.ArrivalTimeUtc).IsRequired();
        builder.Property(visit => visit.DepartureTimeUtc);
        builder.Property(visit => visit.DurationMinutes);
        builder.Property(visit => visit.DistanceFromCustomerMeters)
            .HasPrecision(10, 2);
    }
}
