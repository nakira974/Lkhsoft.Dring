namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC response message
/// </summary>
public class ResponseMessage : MessageBase<ResponseMessageHeader>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public ResponseMessage() : base()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="responseType">Response message type</param>
    /// <param name="data">Response message data</param>
    public ResponseMessage(MessageType type, ResponseMessageType responseType, object data) : base(data)
    {
        Header = new ResponseMessageHeader(type, responseType, (ushort) Body.Length);
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="responseType">Response message type</param>
    /// <param name="data">Response message data</param>
    /// <param name="offset">Response message data offset</param>
    /// <param name="count">Response message data count</param>
    public ResponseMessage(MessageType type, ResponseMessageType responseType, byte[] data, int offset, int count) : base(data, offset, count)
    {
        Header = new ResponseMessageHeader(type, responseType, (ushort) count);
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="typeonseType">Response message type</param>
    /// <param name="data">Response message data</param>
    /// <param name="offset">Response message data offset</param>
    public ResponseMessage(MessageType type, ResponseMessageType responseType, MemoryStream data, int offset = 0) : base(data, offset)
    {
        Header = new ResponseMessageHeader(type, responseType, (ushort) (data.Length - offset));
    }
}