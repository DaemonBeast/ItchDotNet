using System.Text.Json.Serialization;

namespace ItchDotNet.Structs;

// TODO: Finish later.
public record Upload(
    [property: JsonPropertyName("game_id")] long GameId);
