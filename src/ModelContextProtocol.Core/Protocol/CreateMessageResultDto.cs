using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModelContextProtocol.Protocol;

internal sealed class CreateMessageResultDto_V2 // V1, V2, V3, etc. as a placeholder naming convention.
                                                // Could also be the protocol version introducing the breaking change
{
    [JsonPropertyName("content")]
    public required List<ContentBlock> Content { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("stopReason")]
    public string? StopReason { get; init; }

    [JsonPropertyName("role")]
    public required Role Role { get; init; }

    [JsonPropertyName("_meta")]
    public JsonObject? Meta { get; init; }

    /// <summary>
    /// The serializer for <see cref="CreateMessageResult"/> using this DTO.
    /// </summary>
    public static IMcpModelSerializer<CreateMessageResult> ModelSerializer { get; } =
        McpModelSerializer.CreateDtoSerializer<CreateMessageResult, CreateMessageResultDto_V2>(
            toDto: static model => new ()
            {
                Content = model.Contents,
                Model = model.Model,
                StopReason = model.StopReason,
                Role = model.Role,
                Meta = model.Meta
            },
            fromDto: static dto => new()
            {
                Contents = dto.Content,
                Model = dto.Model,
                StopReason = dto.StopReason,
                Role = dto.Role,
                Meta = dto.Meta
            },
            McpJsonUtilities.JsonContext.Default.CreateMessageResultDto_V2);
}

internal sealed class CreateMessageResultDto_V1 // V1, V2, V3, etc. as a placeholder naming convention.
{
    [JsonPropertyName("content")]
    public required ContentBlock Content { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("stopReason")]
    public string? StopReason { get; init; }

    [JsonPropertyName("role")]
    public required Role Role { get; init; }

    [JsonPropertyName("_meta")]
    public JsonObject? Meta { get; init; }

    /// <summary>
    /// The serializer for <see cref="CreateMessageResult"/> using this DTO.
    /// </summary>
    public static IMcpModelSerializer<CreateMessageResult> ModelSerializer { get; } =
        McpModelSerializer.CreateDtoSerializer<CreateMessageResult, CreateMessageResultDto_V1>(
            toDto: static model => new()
            {
                Content = model.Content,
                Model = model.Model,
                StopReason = model.StopReason,
                Role = model.Role,
                Meta = model.Meta
            },
            fromDto: static dto => new()
            {
                Contents = [dto.Content],
                Model = dto.Model,
                StopReason = dto.StopReason,
                Role = dto.Role,
                Meta = dto.Meta
            },
            McpJsonUtilities.JsonContext.Default.CreateMessageResultDto_V1);
}
