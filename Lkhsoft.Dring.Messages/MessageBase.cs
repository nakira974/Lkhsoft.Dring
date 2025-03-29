using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC message base
/// </summary>
public abstract class MessageBase<THeader> : IComparable<MessageBase<THeader>> where THeader : MessageHeaderBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MessageBase()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="data">Data object</param>
    protected MessageBase(object data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Message object data cannot be null");
        }

        var memoryStream = new MemoryStream();
        
        try
        {
            JsonSerializer.Serialize(new Utf8JsonWriter(memoryStream), data);
            memoryStream.Position = 0;
        }
        catch (Exception ex)
        {
            memoryStream.Dispose();
            throw new InvalidOperationException("Error while serializing message data", ex);
        }
        Body = new MessageBody(memoryStream);
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="data">Message data</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="count">Buffer length</param>
    protected MessageBase(byte[] data, int offset, int count)
    {
        Body = new MessageBody(data, offset, count);
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="data">Buffer data</param>
    /// <param name="offset">Buffer offset</param>
    protected MessageBase(MemoryStream data, int offset = 0)
    {
        Body = new MessageBody(data, offset);
    }

    /// <summary>
    /// Message type
    /// </summary>
    [JsonPropertyName("header")]
    public THeader? Header { get; init; }
    
    /// <summary>
    /// Message body
    /// </summary>
    [JsonPropertyName("body")]
    public MessageBody Body { get; init; }
    

    /// <summary>
    ///     Compares this message with another message based on ClientId
    /// </summary>
    /// <param name="other">The other message to compare</param>
    /// <returns>An integer indicating the relative order of the messages</returns>
    public int CompareTo(MessageBase<THeader>? other)
    {
        return String.Compare(this.Header.MessageId, other?.Header.MessageId, StringComparison.Ordinal);
    }
}