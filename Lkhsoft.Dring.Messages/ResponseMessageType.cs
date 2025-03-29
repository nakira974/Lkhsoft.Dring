namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC response message type
/// </summary>
public enum ResponseMessageType
{
    /// <summary>
    /// No response
    /// </summary>
    None = 0,

    /// <summary>
    /// Success response
    /// </summary>
    Success = 1,

    /// <summary>
    /// Error response
    /// </summary>
    Error = 2,

    /// <summary>
    /// Warning response
    /// </summary>
    Warning = 3
}