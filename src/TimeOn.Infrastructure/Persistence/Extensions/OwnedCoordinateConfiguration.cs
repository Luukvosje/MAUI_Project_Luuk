using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Infrastructure.Persistence.Extensions;

internal static class OwnedCoordinateConfiguration
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, Coordinate> coordinate)
        where TOwner : class
    {
        coordinate.Property(c => c.Latitude).IsRequired();
        coordinate.Property(c => c.Longitude).IsRequired();
    }
}
