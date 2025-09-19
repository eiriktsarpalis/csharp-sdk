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
    public string? StopReason { get; init; }

    /// <summary>
    /// Gets or sets the role of the user who generated the message.
    /// </summary>
    public required Role Role { get; init; }

    /// <summary>
    /// A <see cref="CreateMessageResult"/> serializer that delegates to the appropriate versioned DTO serializer
    /// </summary>
    internal static IMcpModelSerializer<CreateMessageResult> ModelSerializer { get; } =
        McpModelSerializer.CreateDelegatingSerializer(endpoint =>
        {
            if (endpoint?.NegotiatedProtocolVersion is string version &&
                DateTime.Parse(version) < new DateTime(2025, 09, 18)) // A hypothetical future version
            {
                // The negotiated protocol version is before 2025-09-18, so we need to use the V1 serializer.
                return CreateMessageResultDto_V1.ModelSerializer;
            }
            else
            {
                return CreateMessageResultDto_V2.ModelSerializer;
            }
        });
}