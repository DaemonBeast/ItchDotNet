using System.Net.Http.Headers;
using System.Threading.RateLimiting;

namespace ItchDotNet;

public class ItchClientHandler
{
    public string? ApiKey { get; }

    private readonly HttpClient _httpClient;
    private readonly ItchClientRateLimiter _rateLimiter;

    public ItchClientHandler(HttpClient httpClient, ItchClientRateLimiter rateLimiter, string? apiKey = null)
    {
        _httpClient = httpClient;
        _rateLimiter = rateLimiter;

        ApiKey = apiKey;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
    }

    public async Task<HttpResponseMessage> GetAsync(string uri)
    {
        using var lease = await _rateLimiter.RateLimiter.AcquireAsync();
        if (!lease.IsAcquired) throw new Exception("Failed to acquire rate limiter lease.");

        return await _httpClient.GetAsync(uri);
    }

    public async Task<HttpResponseMessage> PostAsync(string uri, FormUrlEncodedContent content)
    {
        using var lease = await _rateLimiter.RateLimiter.AcquireAsync();
        if (!lease.IsAcquired) throw new Exception("Failed to acquire rate limiter lease.");

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = content;

        return await _httpClient.SendAsync(request);
    }
}

public class ItchClientRateLimiter
{
    public TokenBucketRateLimiter RateLimiter { get; } = new(new TokenBucketRateLimiterOptions
    {
        AutoReplenishment = true,
        QueueLimit = int.MaxValue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        TokenLimit = 20,
        TokensPerPeriod = 8
    });
}

public class ItchClientHandlerFactory(
    IHttpClientFactory httpClientFactory,
    ItchClientRateLimiter rateLimiter) : IItchClientHandlerFactory
{
    public ItchClientHandler Create(string? apiKey = null)
        => new(httpClientFactory.CreateClient(), rateLimiter, apiKey);
}

public interface IItchClientHandlerFactory
{
    public ItchClientHandler Create(string? apiKey = null);
}
