using System.Text.Json;
using ItchDotNet.Structs;

namespace ItchDotNet;

public partial class ItchClient
{
    public class ProfileMethods(ItchClient client)
    {
        public async IAsyncEnumerable<DownloadKey> OwnedKeys()
        {
            if (client._handler.ApiKey == null) throw new Exception("Must be logged in to get owned keys.");

            var page = 1;

            while (true)
            {
                // TODO: Query builder that supports values other than strings?
                var uriBuilder = new UriBuilder(Constants.Endpoints.Itch.Api.Profile.OwnedKeys)
                {
                    Query = $"?page={page}"
                };

                var response = await client._handler.GetAsync(uriBuilder.ToString());

                using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                var root = document.RootElement;
                var perPage = root.GetProperty("per_page").GetInt64();
                var numIterated = 0;

                var keys = root.GetProperty("owned_keys").EnumerateArray();
                foreach (var key in keys)
                {
                    numIterated++;
                    yield return key.Deserialize<DownloadKey>()!;
                }

                if (numIterated < perPage)
                {
                    yield break;
                }

                page++;
            }
        }
    }
}
