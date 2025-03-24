#region

using System.Text.Json.Serialization;

#endregion

namespace Lkhsoft.Dring.Messages;

/// <summary>
///     Represents a message with metadata for prioritization
/// </summary>
public class Message : IComparable<Message>
{
    /// <summary>
    /// Default constructor for serialization purposes only
    /// </summary>
    public Message()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="data">Message data</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="count">Buffer length</param>
    public Message(MessageType type, byte[] data, int offset, int count)
    {
        MessageType = type;
        MessageId = Guid.NewGuid().ToString("X");
        Data = new byte[count];
        var stream = new MemoryStream(data, offset, count);
        stream.ReadExactly(Data, offset, count);
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="data">Buffer data</param>
    /// <param name="offset">Buffer offset</param>
    public Message(MessageType type, MemoryStream data, int offset = 0)
    {
        if (!data.CanRead || data.Length == 0)
            throw new ArgumentException("Stream must be readable and non-empty", nameof(data));

        MessageType = type;
        MessageId = Guid.NewGuid().ToString("X");

        Data = new byte[data.Length];
        data.Position = 0;
        data.ReadExactly(Data, offset, Data.Length);
    }

    /// <summary>
    /// Message type
    /// </summary>
    [JsonPropertyName("messageType")]
    public MessageType MessageType { get; init; }

    /// <summary>
    /// Message identifier
    /// </summary>
    [JsonPropertyName("messageId")]
    public string? MessageId { get; init; }

    /// <summary>
    /// Message data
    /// </summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; init; }

    /// <summary>
    ///     Compares this message with another message based on ClientId
    /// </summary>
    /// <param name="other">The other message to compare</param>
    /// <returns>An integer indicating the relative order of the messages</returns>
    public int CompareTo(Message? other)
    {
        return MessageType.CompareTo(other?.MessageType);
    }
}