namespace TimeOn.Mobile.Interfaces;

public interface IApiService
{
    string? LastError { get; }

    Task<TResponse?> GetAsync<TResponse>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
    Task DeleteAsync(string endpoint);
}
