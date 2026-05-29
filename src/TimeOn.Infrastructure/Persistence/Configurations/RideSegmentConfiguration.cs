using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TimeOn.Domain.Entities;



namespace TimeOn.Infrastructure.Persistence.Configurations;



public sealed class RideSegmentConfiguration : IEntityTypeConfiguration<RideSegment>

{

    public void Configure(EntityTypeBuilder<RideSegment> builder)

    {

        builder.ToTable("RideSegments");

        builder.HasKey(segment => segment.Id);



        builder.Property(segment => segment.WorkSessionId).IsRequired();

        builder.Property(segment => segment.StartTimeUtc).IsRequired();

        builder.Property(segment => segment.EndTimeUtc);

        builder.Property(segment => segment.DistanceMeters).IsRequired();

    }

}

