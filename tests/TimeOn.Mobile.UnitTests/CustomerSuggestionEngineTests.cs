using TimeOn.Mobile.Core.Models;
using TimeOn.Mobile.Core.UseCases;

namespace TimeOn.Mobile.UnitTests;

public sealed class CustomerSuggestionEngineTests
{
    [Fact]
    public void SuggestCustomer_ReturnsNearestCustomer()
    {
        var engine = new CustomerSuggestionEngine();
        var current = new LocationSample(52.3676, 4.9041, 0, DateTimeOffset.UtcNow);
        var customers = new List<(string CustomerName, double Latitude, double Longitude)>
        {
            ("Client Rotterdam", 51.9225, 4.4792),
            ("Client Amsterdam", 52.3677, 4.9043),
        };

        string result = engine.SuggestCustomer(current, customers);

        Assert.Equal("Client Amsterdam", result);
    }
}
