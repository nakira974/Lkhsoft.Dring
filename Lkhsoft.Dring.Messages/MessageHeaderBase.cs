using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// Header for RPC messages
/// </summary>
public abstract class MessageHeaderBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MessageHeaderBase()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="length">Message data length</param>
    protected MessageHeaderBase(MessageType type, ushort length)
    {
        MessageType = type;
        MessageId = Guid.NewGuid().ToString("X");;
        MessageLength = length;
    }
    
    [JsonPropertyName("type")]
    public MessageType? MessageType { get; init; }
    
    /// <summary>
    /// RPC message identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string? MessageId { get; init; }
    
    /// <summary>
    /// RPC message length
    /// </summary>
    [JsonPropertyName("length")]
    public ushort? MessageLength { get; init; }
}