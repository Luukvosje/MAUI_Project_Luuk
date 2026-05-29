namespace TimeOn.Infrastructure.Configurations;


public sealed class LocalDatabaseSettings
{
    public const string SectionName = "LocalDatabase";
    public string ConnectionString { get; init; } = "Data Source=timeon-local.db";
}
