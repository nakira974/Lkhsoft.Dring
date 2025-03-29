using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC request message header
/// </summary>
public class RequestMessageHeader : MessageHeaderBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RequestMessageHeader() : base()
    {
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="requestType">Request message type</param>
    /// <param name="length">Request message length</param>
    public RequestMessageHeader(MessageType type, RequestMessageType requestType, ushort length) : base(type, length)
    {
        RequestType = requestType;
    }
    
    /// <summary>
    /// RPC message request type
    /// </summary>
    [JsonPropertyName("request_type")]
    public RequestMessageType RequestType { get; init; }
}