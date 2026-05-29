namespace TimeOn.Infrastructure.Configurations;

public sealed class GoogleApiSettings
{
    public const string SectionName = "GoogleApi";

    public string ApiKey { get; init; } = string.Empty;
}
