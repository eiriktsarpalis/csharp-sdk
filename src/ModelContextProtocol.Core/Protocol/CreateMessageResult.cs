using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModelContextProtocol.Protocol;

/// <summary>
/// Represents a client's response to a <see cref="RequestMethods.SamplingCreateMessage"/> from the server.
/// </summary>
/// <remarks>
/// See the <see href="https://github.com/modelcontextprotocol/specification/blob/main/schema/">schema</see> for details.
/// </remarks>
public sealed class CreateMessageResult : Result
{
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonConverter(typeof(SingleOrArrayContentConverter))]
    public required List<ContentBlock> Contents
    {
        get;
        init
        { 
            if (value is null or [])
            {
                throw new ArgumentException(nameof(Contents));
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [JsonIgnore]
    public ContentBlock Content { get => Contents.First(); init => Contents = [value]; }

    /// <summary>
    /// Gets or sets the name of the model that generated the message.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This should contain the specific model identifier such as "claude-3-5-sonnet-20241022" or "o3-mini".
    /// </para>
    /// <para>
    /// This property allows the server to know which model was used to generate the response,
    /// enabling appropriate handling based on the model's capabilities and characteristics.
    /// </para>
    /// </remarks>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Gets or sets the reason why message generation (sampling) stopped, if known.
    /// </summary>
    /// <remarks>
    /// Common values include:
    /// <list type="bullet">
    ///   <item><term>endTurn</term><description>The model naturally completed its response.</description></item>
    ///   <item><term>maxTokens</term><description>The response was truncated due to reaching token limits.</description></item>
    ///   <item><term>stopSequence</term><description>A specific stop sequence was encountered during generation.</description></item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("stopReason")]
    public string? StopReason { get; init; }

    /// <summary>
    /// Gets or sets the role of the user who generated the message.
    /// </summary>
    [JsonPropertyName("role")]
    public required Role Role { get; init; }

    /// <summary>
    /// Defines a converter that handles deserialization of a single <see cref="ContentBlock"/> or an array of <see cref="ContentBlock"/> into a <see cref="List{ContentBlock}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SingleOrArrayContentConverter : McpJsonUtilities.McpContextualJsonConverter<List<ContentBlock>>
    {
        /// <inheritdoc/>
        public override List<ContentBlock>? Read(ref Utf8JsonReader reader, McpSession? session, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.StartObject)
            {
                var single = JsonSerializer.Deserialize(ref reader, options.GetTypeInfo<ContentBlock>());
                return [single];
            }

            return JsonSerializer.Deserialize(ref reader, options.GetTypeInfo<List<ContentBlock>>());
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, List<ContentBlock> value, McpSession? session, JsonSerializerOptions options)
        {
            if (session?.NegotiatedProtocolVersion is string version &&
                DateTime.Parse(version) < new DateTime(2025, 09, 18)) // A hypothetical future version
            {
                // The negotiated protocol version is before 2025-09-18, so we need to serialize as a single object.
                JsonSerializer.Serialize(value.Single(), options.GetTypeInfo<ContentBlock>());
            }

            JsonSerializer.Serialize(value, options.GetTypeInfo<List<ContentBlock>>());
        }
    }
}
