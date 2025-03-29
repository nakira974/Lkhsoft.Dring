#region

using System.Text.Json.Serialization;

#endregion

namespace Lkhsoft.Dring.Messages;

/// <summary>
///     Represents a RPC message with metadata for prioritization
/// </summary>
public class RequestMessage : MessageBase<RequestMessageHeader>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RequestMessage() : base()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="requestType">Request message type</param>
    /// <param name="data">Request message data</param>
    public RequestMessage(MessageType type, RequestMessageType requestType, object data) : base(data)
    {
        Header = new RequestMessageHeader(type, requestType, (ushort) Body.Length);
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="requestType">Request message type</param>
    /// <param name="data">Request message data</param>
    /// <param name="offset">Request message data offset</param>
    /// <param name="count">Request message data count</param>
    public RequestMessage(MessageType type, RequestMessageType requestType, byte[] data, int offset, int count) : base(data, offset, count)
    {
        Header = new RequestMessageHeader(type, requestType,(ushort) count);
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="requestType">Request message type</param>
    /// <param name="data">Request message data</param>
    /// <param name="offset">Request message data offset</param>
    public RequestMessage(MessageType type, RequestMessageType requestType, MemoryStream data, int offset = 0) : base(data, offset)
    {
        Header = new RequestMessageHeader(type, requestType, (ushort) (data.Length - offset));
    }
}