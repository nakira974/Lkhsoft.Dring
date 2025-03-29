using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC message body
/// </summary>
public readonly struct MessageBody
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public MessageBody()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="data">Message data</param>
    /// <param name="offset">Buffer offset</param>
    /// <param name="count">Buffer count</param>
    public MessageBody(byte[] data, int offset, int count)
    {
        Data = new byte[count];
        var stream = new MemoryStream(data, offset, count);
        stream.ReadExactly(Data, offset, count);
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="data">Message data</param>
    /// <param name="offset">Buffer offset</param>
    /// <exception cref="ArgumentException">The stream is empty or unreadble</exception>
    public MessageBody(MemoryStream data, int offset = 0)
    {
        if (!data.CanRead || data.Length == 0)
            throw new ArgumentException("Stream must be readable and non-empty", nameof(data));

        Data = new byte[data.Length];
        data.Position = 0;
        data.ReadExactly(Data, offset, Data.Length);
    }
    
    /// <summary>
    /// Message body data
    /// </summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; init; }
    
    /// <summary>
    /// Message body length
    /// </summary>
    public int Length => Data?.Length ?? 0;
    
    /// <summary>
    /// Reads the message body as a stream
    /// </summary>
    /// <returns>A stream from the message data</returns>
    public Stream ReadAsStream()
    {
        var stream = new MemoryStream(Data ?? throw new InvalidOperationException("Message body is empty"));
        stream.Position = 0;
        return stream;
    }
}