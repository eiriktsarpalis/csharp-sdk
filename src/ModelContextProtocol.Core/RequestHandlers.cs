using ModelContextProtocol.Protocol;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace ModelContextProtocol;

internal sealed class RequestHandlers : Dictionary<string, Func<JsonRpcRequest, CancellationToken, Task<JsonNode?>>>
{
    /// <summary>
    /// Registers a handler for incoming requests of a specific method in the MCP protocol.
    /// </summary>
    /// <typeparam name="TParams">Type of request payload that will be deserialized from incoming JSON</typeparam>
    /// <typeparam name="TResult">Type of response payload that will be serialized to JSON (not full RPC response)</typeparam>
    /// <param name="method">Method identifier to register for (e.g., "tools/list", "logging/setLevel")</param>
    /// <param name="handler">Handler function to be called when a request with the specified method identifier is received</param>
    /// <param name="requestSerializer">The JSON contract governing request parameter deserialization</param>
    /// <param name="responseDeserializer">The JSON contract governing response serialization</param>
    /// <remarks>
    /// <para>
    /// This method is used internally by the MCP infrastructure to register handlers for various protocol methods.
    /// When an incoming request matches the specified method, the registered handler will be invoked with the
    /// deserialized request parameters.
    /// </para>
    /// <para>
    /// The handler function receives the deserialized request object, the full JSON-RPC request, and a cancellation token,
    /// and should return a response object that will be serialized back to the client.
    /// </para>
    /// </remarks>
    public void Set<TParams, TResult>(
        string method,
        Func<TParams?, JsonRpcRequest, CancellationToken, ValueTask<TResult>> handler,
        JsonTypeInfo<TParams> requestSerializer,
        JsonTypeInfo<TResult> responseDeserializer)
    {
        Throw.IfNull(method);
        Throw.IfNull(handler);
        Throw.IfNull(requestSerializer);
        Throw.IfNull(responseDeserializer);

        this[method] = async (request, cancellationToken) =>
        {
            TParams? typedRequest = JsonSerializer.Deserialize(request.Params, requestSerializer);
            TResult? result = await handler(typedRequest, request, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.SerializeToNode(result, responseDeserializer);
        };
    }

    /// <summary>
    /// Registers a handler for incoming requests of a specific method in the MCP protocol.
    /// </summary>
    /// <typeparam name="TParams">Type of request payload that will be deserialized from incoming JSON</typeparam>
    /// <typeparam name="TResult">Type of response payload that will be serialized to JSON (not full RPC response)</typeparam>
    /// <param name="method">Method identifier to register for (e.g., "tools/list", "logging/setLevel")</param>
    /// <param name="session"></param>
    /// <param name="handler">Handler function to be called when a request with the specified method identifier is received</param>
    /// <param name="requestSerializer">The JSON contract governing request parameter deserialization</param>
    /// <param name="responseSerializer">The JSON contract governing response serialization</param>
    /// <remarks>
    /// <para>
    /// This method is used internally by the MCP infrastructure to register handlers for various protocol methods.
    /// When an incoming request matches the specified method, the registered handler will be invoked with the
    /// deserialized request parameters.
    /// </para>
    /// <para>
    /// The handler function receives the deserialized request object, the full JSON-RPC request, and a cancellation token,
    /// and should return a response object that will be serialized back to the client.
    /// </para>
    /// </remarks>
    public void Set<TParams, TResult>(
        string method,
        McpSession session,
        Func<TParams?, JsonRpcRequest, CancellationToken, ValueTask<TResult>> handler,
        IMcpModelSerializer<TParams> requestSerializer,
        IMcpModelSerializer<TResult> responseSerializer)
    {
        Throw.IfNull(method);
        Throw.IfNull(handler);
        Throw.IfNull(requestSerializer);
        Throw.IfNull(responseSerializer);

        this[method] = async (request, cancellationToken) =>
        {
            TParams? typedRequest = requestSerializer.Deserialize(request.Params, session);
            TResult? result = await handler(typedRequest, request, cancellationToken).ConfigureAwait(false);
            return responseSerializer.Serialize(result, session);
        };
    }
}
