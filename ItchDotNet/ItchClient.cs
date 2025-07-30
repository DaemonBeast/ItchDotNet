namespace ItchDotNet;

public partial class ItchClient
{
    public LoginMethods Login { get; }
    public ProfileMethods Profile { get; }
    public UploadsMethods Uploads { get; }

    private readonly ItchClientHandler _handler;

    public static ItchClient CreateWithDefaults(string? apiKey = null)
        => new(new ItchClientHandler(new HttpClient(), new ItchClientRateLimiter(), apiKey));

    public ItchClient(ItchClientHandler handler)
    {
        _handler = handler;

        Login = new LoginMethods(this);
        Profile = new ProfileMethods(this);
        Uploads = new UploadsMethods(this);
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
