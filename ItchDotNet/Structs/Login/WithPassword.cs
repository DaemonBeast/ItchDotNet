using System.Text.Json.Serialization;

namespace ItchDotNet.Structs.Login;

public record WithPassword(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("recaptcha_response")] string? RecaptchaResponse = null,
    [property: JsonPropertyName("force_recaptcha")] bool? ForceRecaptcha = null)
{
    public record Response(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("key")] Response.KeyInfo Key,
        [property: JsonPropertyName("cookie")] Response.CookieInfo Cookie)
    {
        public record KeyInfo(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("user_id")] long UserId,
            [property: JsonPropertyName("key")] string Key,
            [property: JsonPropertyName("revoked")] bool Revoked,
            [property: JsonPropertyName("source")] string Source,
            [property: JsonPropertyName("source_version")] string SourceVersion,
            [property: JsonPropertyName("created_at")] string CreatedAt,
            [property: JsonPropertyName("updated_at")] string UpdatedAt,
            [property: JsonPropertyName("last_used_at")] string LastUsedAt);

        public record CookieInfo(
            [property: JsonPropertyName("itchio")] string ItchIo);
    }
}
