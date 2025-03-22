namespace Lkhsoft.Dring.Messages;

/// <summary>
/// Message type enumeration
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Disconnect message
    /// </summary>
    Disconnect = 0x0,

    /// <summary>
    /// Authentication message
    /// </summary>
    Authentication = 0x1,

    /// <summary>
    /// Update routing table message
    /// </summary>
    UpdateRoutingTable = 0xA,

    /// <summary>
    /// Content message
    /// </summary>
    Content = 0x32,

    /// <summary>
    /// Streaming message
    /// </summary>
    Streaming = 0x400
}