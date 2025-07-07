using System.Text.Json.Serialization;

namespace ItchDotNet.Structs;

public record DownloadKey(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("game_id")] long GameId,
    // [property: JsonPropertyName("game")] Game Game,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("owner_id")] long OwnerId);

// TODO: "owner_id" is missing in "/profile/owned-keys" response but there's "downloads" and "purchase_id"?
