// using System.Net.Http.Json;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using TimeOn.Domain.Entities;
// using TimeOn.Infrastructure.Configurations;

// namespace TimeOn.Infrastructure.External;

// public sealed class TimeOnApiClient
// {
//     private readonly HttpClient _httpClient;
//     private readonly TimeOnApiSettings _settings;
//     private readonly ILogger<TimeOnApiClient> _logger;

//     public TimeOnApiClient(
//         HttpClient httpClient,
//         IOptions<TimeOnApiSettings> settings,
//         ILogger<TimeOnApiClient> logger)
//     {
//         _httpClient = httpClient;
//         _settings = settings.Value;
//         _logger = logger;

//         if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
//         {
//             _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
//         }
//     }

//     public async Task<IReadOnlyList<Customer>> GetCustomersAsync()
//     {
//         if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
//         {
//             _logger.LogWarning("Time On API base URL is not configured. Returning an empty customer list.");
//             return [];
//         }

//         if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
//         {
//             _httpClient.DefaultRequestHeaders.Authorization =
//                 new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
//         }

//         var response = await _httpClient.GetAsync("api/customers", cancellationToken);
//         if (!response.IsSuccessStatusCode)
//         {
//             _logger.LogWarning(
//                 "Time On API returned {StatusCode} when fetching customers.",
//                 response.StatusCode);
//             return [];
//         }

//         var payloads = await response.Content.ReadFromJsonAsync<List<TimeOnCustomerDto>>(cancellationToken)
//             ?? [];

//         var syncedAtUtc = DateTime.UtcNow;
//         return payloads
//             .Select(dto => Customer.FromExternal(
//                 dto.Id,
//                 dto.Name,
//                 dto.Address,
//                 dto.Latitude,
//                 dto.Longitude,
//                 syncedAtUtc))
//             .ToList();
//     }

//     private sealed class TimeOnCustomerDto
//     {
//         public Guid Id { get; init; }
//         public string Name { get; init; } = string.Empty;
//         public string? Address { get; init; }
//         public double Latitude { get; init; }
//         public double Longitude { get; init; }
//     }
// }
