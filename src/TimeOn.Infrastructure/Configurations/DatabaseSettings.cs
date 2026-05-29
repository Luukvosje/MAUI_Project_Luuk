namespace TimeOn.Infrastructure.Configurations;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = "SqlServer";
    public string ConnectionString { get; init; } = string.Empty;
}
