namespace TimeOn.Infrastructure.Persistence;

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}
