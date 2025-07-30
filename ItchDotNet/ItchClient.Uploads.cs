using System.Text.Json;
using ItchDotNet.Structs;

namespace ItchDotNet;

public partial class ItchClient
{
    public class UploadsMethods(ItchClient client)
    {
        public async Task<Upload> Upload(long uploadId)
        {
            var uri = string.Format(Constants.Endpoints.Itch.Api.Uploads.Upload, uploadId);
            var response = await client._handler.GetAsync(uri);

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = document.RootElement;

            return root.GetProperty("upload").Deserialize<Upload>()!;
        }

        // TODO: Implement caching so that the "downloadKeyId" doesn't have to be passed in.
        public async Task<Build> Build(long buildId, long downloadKeyId)
        {
            var baseUri = string.Format(Constants.Endpoints.Itch.Api.Uploads.Build, buildId);
            var uriBuilder = new UriBuilder(baseUri)
            {
                Query = $"?download_key_id={downloadKeyId}"
            };

            var response = await client._handler.GetAsync(uriBuilder.ToString());

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = document.RootElement;

            return root.GetProperty("build").Deserialize<Build>()!;
        }

        public async IAsyncEnumerable<Build> Builds(long uploadId)
        {
            var uri = string.Format(Constants.Endpoints.Itch.Api.Uploads.Builds, uploadId);
            var response = await client._handler.GetAsync(uri);

            Build? lastBuild = null;
            using (var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()))
            {
                var root = document.RootElement;
                var builds = root.GetProperty("builds").EnumerateArray();

                foreach (var build in builds)
                {
                    lastBuild = build.Deserialize<Build>()!;
                    yield return lastBuild;
                }
            }

            if (lastBuild == null) yield break;

            var upload = await Upload(uploadId);
            var downloadKey = client.Profile.OwnedKeys().ToBlockingEnumerable().First(k => k.GameId == upload.GameId);

            var parentBuildId = lastBuild.ParentBuildId;
            while (parentBuildId != -1)
            {
                var build = await Build(parentBuildId, downloadKey.Id);
                yield return build;

                parentBuildId = build.ParentBuildId;
            }
        }
    }
}
