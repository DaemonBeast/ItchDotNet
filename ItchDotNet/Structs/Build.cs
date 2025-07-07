using System.Text.Json.Serialization;

namespace ItchDotNet.Structs;

public record Build(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("parent_build_id")] long ParentBuildId,
    [property: JsonPropertyName("state")] string State, // TODO: Turn into enum.
    [property: JsonPropertyName("version")] long Version,
    [property: JsonPropertyName("user_version")] string UserVersion, // TODO: Seemingly missing?
    // [property: JsonPropertyName("files")] File[] Files,
    // [property: JsonPropertyName("user")] User User,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

// TODO: There's also the `upload_id`?
