using GoogleMaps.LocationServices;
using Microsoft.Extensions.Options;
using TimeOn.Domain.Shared;
using TimeOn.Application.Interfaces;
using TimeOn.Infrastructure.Configurations;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Infrastructure.External
{
    public sealed class GoogleGeocodingService : IGeoLocationService
    {
        private readonly GoogleApiSettings _settings;

        public GoogleGeocodingService(IOptions<GoogleApiSettings> settings)
        {
            _settings = settings.Value;
        }

        public Task<Result<Coordinate>> AdressToCoordinate(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    return Task.FromResult(Result<Coordinate>.Failure("Google API key is not configured."));
                }

                var client = new GoogleLocationService(_settings.ApiKey);
                var point = client.GetLatLongFromAddress(address);

                if (point == null)
                {
                    return Task.FromResult(Result<Coordinate>.Failure("Unable to geocode the provided address."));
                }

                return Task.FromResult(Result<Coordinate>.Success(Coordinate.Create(point.Latitude, point.Longitude)));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result<Coordinate>.Failure($"Error occurred while geocoding the address: {ex.Message}"));
            }
        }
    }
}
