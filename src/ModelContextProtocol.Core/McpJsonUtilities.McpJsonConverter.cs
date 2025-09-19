using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ModelContextProtocol;

public static partial class McpJsonUtilities
{
    [ThreadStatic]
    private static McpSession? t_currentMcpSession;

    /// <summary>
    /// Serializes the given value to a <see cref="JsonNode"/> using the provided <see cref="JsonTypeInfo{T}"/>,
    /// </summary>
    internal static JsonNode? SerializeContextual<T>(T? value, JsonTypeInfo<T> typeInfo, McpSession session)
    {
        if (session is null)
        {
            Throw.IfNull(session);
        }

        if (t_currentMcpSession is not null)
        {
            throw new InvalidOperationException("Reentrant call to McpJsonUtilities.SerializeContextual detected.");
        }

        t_currentMcpSession = session;
        try
        {
            return JsonSerializer.SerializeToNode(value!, typeInfo);
        }
        finally
        {
            t_currentMcpSession = null;
        }
    }

    /// <summary>
    /// Deserializes the given value to a <see cref="JsonNode"/> using the provided <see cref="JsonTypeInfo{T}"/>,
    /// </summary>
    internal static T? DeserializeContextual<T>(JsonNode? node, JsonTypeInfo<T> typeInfo, McpSession session)
    {
        if (session is null)
        {
            Throw.IfNull(session);
        }

        if (t_currentMcpSession is not null)
        {
            throw new InvalidOperationException("Reentrant call to McpJsonUtilities.DeserializeContextual detected.");
        }

        t_currentMcpSession = session;
        try
        {
            return JsonSerializer.Deserialize(node, typeInfo);
        }
        finally
        {
            t_currentMcpSession = null;
        }
    }

    /// <summary>
    /// Defines an abstract JSON converter that has access to the current <see cref="IMcpEndpoint"/> context during serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The type being converted.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class McpContextualJsonConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Reads the JSON representation of the value.
        /// </summary>
        public abstract T? Read(ref Utf8JsonReader reader, McpSession? session, JsonSerializerOptions options);

        /// <summary>
        /// Writes the JSON representation of the value.
        /// </summary>
        public abstract void Write(Utf8JsonWriter writer, T value, McpSession? session, JsonSerializerOptions options);

        /// <inheritdoc/>
        public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Read(ref reader, t_currentMcpSession, options);

        /// <inheritdoc/>
        public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            Write(writer, value, t_currentMcpSession, options);
    }
}
