using System.Net.Http.Headers;
using System.Threading.RateLimiting;

namespace ItchDotNet;

public partial class ItchClient
{
    public string? ApiKey
    {
        get => _apiKey;
        set
        {
            _apiKey = value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
        }
    }

    public LoginMethods Login { get; }
    public ProfileMethods Profile { get; }
    public UploadsMethods Uploads { get; }

    private string? _apiKey;

    private readonly TokenBucketRateLimiter _rateLimiter;

    private readonly HttpClient _httpClient;

    public static ItchClient CreateWithDefaults()
    {
        return new ItchClient(new HttpClient());
    }

    public ItchClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = true,
            QueueLimit = int.MaxValue,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokenLimit = 20,
            TokensPerPeriod = 8
        });

        Login = new LoginMethods(this);
        Profile = new ProfileMethods(this);
        Uploads = new UploadsMethods(this);
    }

    private async Task<HttpResponseMessage> GetAsync(string uri)
    {
        using var lease = await _rateLimiter.AcquireAsync();
        if (!lease.IsAcquired) throw new Exception("Failed to acquire rate limiter lease.");

        return await _httpClient.GetAsync(uri);
    }

    private async Task<HttpResponseMessage> PostAsync(string uri, FormUrlEncodedContent content)
    {
        using var lease = await _rateLimiter.AcquireAsync();
        if (!lease.IsAcquired) throw new Exception("Failed to acquire rate limiter lease.");

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = content;

        return await _httpClient.SendAsync(request);
    }

    public class ContentBuilder : IReadOnlyContentBuilder
    {
        private readonly List<KeyValuePair<string, string>> _parameters;

        public ContentBuilder()
        {
            _parameters = [];
        }

        private ContentBuilder(List<KeyValuePair<string, string>> parameters)
        {
            _parameters = parameters;
        }

        public ContentBuilder Add(string key, string value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public ContentBuilder Add(string key, bool value)
        {
            if (value)
            {
                _parameters.Add(new KeyValuePair<string, string>(key, string.Empty));
            }

            return this;
        }

        public IReadOnlyContentBuilder Freeze()
            => this;

        public FormUrlEncodedContent Build()
            => new(_parameters);

        public ContentBuilder Clone()
            => new([.._parameters]);
    }

    public interface IReadOnlyContentBuilder
    {
        public FormUrlEncodedContent Build();

        public ContentBuilder Clone();
    }
}
