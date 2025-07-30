using System.Text.Json;

namespace ItchDotNet;

public partial class ItchClient
{
    public class LoginMethods(ItchClient client)
    {
        public delegate Task<string> RecaptchaNeededHandler(string recaptchaUrl);

        public async Task<Structs.Login.WithPassword.Response> WithPassword(
            string username,
            string password,
            RecaptchaNeededHandler onRecaptchaNeeded,
            bool forceRecaptcha = false,
            CancellationToken cancellationToken = default)
        {
            var baseContentBuilder = new ContentBuilder()
                .Add("source", "desktop")
                .Add("username", username)
                .Add("password", password)
                .Freeze();

            var content = baseContentBuilder.Clone()
                .Add("force_recaptcha", forceRecaptcha)
                .Build();

            var response = await client._handler.PostAsync(Constants.Endpoints.Itch.Api.Login.WithPassword, content);

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            var root = document.RootElement;

            var success = root.GetProperty("success").GetBoolean();
            if (success)
            {
                return document.Deserialize<Structs.Login.WithPassword.Response>()!;
            }

            if (root.TryGetProperty("recaptcha_needed", out var recaptchaNeededElement) &&
                recaptchaNeededElement.GetBoolean())
            {
                var recaptchaUrl = root.GetProperty("recaptcha_url").GetString()!;
                var recaptchaToken = await onRecaptchaNeeded.Invoke(recaptchaUrl);

                var recaptchaContent = baseContentBuilder.Clone()
                    .Add("recaptcha_response", recaptchaToken)
                    .Build();

                var recaptchaResponse =
                    await client._handler.PostAsync(Constants.Endpoints.Itch.Api.Login.WithPassword, recaptchaContent);

                using var recaptchaDocument =
                    await JsonDocument.ParseAsync(
                        await recaptchaResponse.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken);

                var recaptchaRoot = recaptchaDocument.RootElement;

                if (recaptchaRoot.TryGetProperty("success", out _))
                {
                    return (await JsonSerializer.DeserializeAsync<Structs.Login.WithPassword.Response>(
                        await recaptchaResponse.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken))!;
                }

                var errors = recaptchaRoot.GetProperty("errors").EnumerateArray().Select(e => e.GetString());
                throw new AggregateException(errors.Select(e => new Exception(e)));
            }

            // TODO: TOTP

            throw new Exception("Failed to handle login response.");
        }
    }
}
