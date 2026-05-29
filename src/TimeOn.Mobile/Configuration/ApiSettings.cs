namespace TimeOn.Mobile.Configuration;

public sealed class ApiSettings
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = "https://localhost:5001/";
}
