using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC response message header
/// </summary>
public class ResponseMessageHeader : MessageHeaderBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public ResponseMessageHeader() : base()
    {
        
    }
    
    /// <inheritdoc/>
    public ResponseMessageHeader(MessageType type, ResponseMessageType responseType, ushort length) : base(type, length)
    {
        Type = responseType;
    }
    
    /// <summary>
    /// RPC message response type
    /// </summary>
    [JsonPropertyName("response_type")]
    public ResponseMessageType? Type { get; init; }
}