using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace ModelContextProtocol.Protocol;

/// <summary>
/// An abstraction for serializing and deserializing MCP model objects to/from JSON,
/// </summary>
/// <typeparam name="TModel">The model type being serialized.</typeparam>
internal interface IMcpModelSerializer<TModel>
{
    public JsonNode? Serialize(TModel? model, McpSession session);
    public TModel? Deserialize(JsonNode? node, McpSession session);
}

/// <summary>
/// Defines a set of factory methods for creating <see cref="IMcpModelSerializer{TModel}"/> instances.
/// </summary>
internal static class McpModelSerializer
{
    /// <summary>
    /// Creates an MCP model serializer that delegates to different serializers based on the MCP session.
    /// </summary>
    public static IMcpModelSerializer<TModel> CreateDelegatingSerializer<TModel>(Func<McpSession, IMcpModelSerializer<TModel>> selector) =>
        new DelegatingMcpModelSerializer<TModel>(selector);

    /// <summary>
    /// Creates an MCP model serializer mapped from a <see cref="JsonTypeInfo{T}"/>.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    public static IMcpModelSerializer<TModel> ToMcpModelSerializer<TModel>(this JsonTypeInfo<TModel> typeInfo) =>
        new JsonTypeInfoMcpModelSerializer<TModel>(typeInfo);

    /// <summary>
    /// Creates an MCP model serializer that maps between a model type and a DTO type for serialization.
    /// </summary>
    /// <typeparam name="TModel">The model type to serialize.</typeparam>
    /// <typeparam name="TDto">The DTO used to drive serialization.</typeparam>
    /// <param name="toDto">The model-to-dto mapper.</param>
    /// <param name="fromDto">The dto-to-model inverse mapper.</param>
    /// <param name="dtoTypeInfo">The <see cref="JsonTypeInfo"/> governing serialization of the DTO type.</param>
    public static IMcpModelSerializer<TModel> CreateDtoSerializer<TModel, TDto>(
        Func<TModel, TDto> toDto,
        Func<TDto, TModel> fromDto,
        JsonTypeInfo<TDto> dtoTypeInfo) =>
        new DtoMappingMcpModelSerializer<TModel, TDto>(toDto, fromDto, dtoTypeInfo);

    private sealed class JsonTypeInfoMcpModelSerializer<TModel>(JsonTypeInfo<TModel> typeInfo) : IMcpModelSerializer<TModel>
    {
        public TModel? Deserialize(JsonNode? node, McpSession _) => JsonSerializer.Deserialize(node, typeInfo);
        public JsonNode? Serialize(TModel? model, McpSession _) => model is null ? null : JsonSerializer.SerializeToNode(model, typeInfo);
    }

    private sealed class DelegatingMcpModelSerializer<TModel>(Func<McpSession, IMcpModelSerializer<TModel>> selector) : IMcpModelSerializer<TModel>
    {
        public TModel? Deserialize(JsonNode? node, McpSession session)
        {
            var serializer = selector(session);
            return serializer.Deserialize(node, session);
        }

        public JsonNode? Serialize(TModel? model, McpSession session)
        {
            var serializer = selector(session);
            return serializer.Serialize(model, session);
        }
    }

    private sealed class DtoMappingMcpModelSerializer<TModel, TDto>(Func<TModel, TDto> toDto, Func<TDto, TModel> fromDto, JsonTypeInfo<TDto> dtoTypeInfo) : IMcpModelSerializer<TModel>
    {
        public JsonNode? Serialize(TModel? model, McpSession _)
        {
            if (model is null)
            {
                return null;
            }

            TDto dto = toDto(model);
            return JsonSerializer.SerializeToNode(dto, dtoTypeInfo);
        }

        public TModel? Deserialize(JsonNode? node, McpSession _)
        {
            TDto? dto = JsonSerializer.Deserialize(node, dtoTypeInfo);
            return dto is null ? default : fromDto(dto);
        }
    }
}
