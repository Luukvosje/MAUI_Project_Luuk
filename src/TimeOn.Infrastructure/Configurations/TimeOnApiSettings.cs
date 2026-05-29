namespace TimeOn.Infrastructure.Configurations;

public sealed class TimeOnApiSettings
{
    public const string SectionName = "TimeOnApi";

    public string BaseUrl { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
}
