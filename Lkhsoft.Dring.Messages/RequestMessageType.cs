namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC request message type
/// </summary>
public enum RequestMessageType
{
    /// <summary>
    /// Login request
    /// </summary>
    Login = 0,
    /// <summary>
    /// Log off request
    /// </summary>
    LogOff = 1,
    /// <summary>
    /// Register request
    /// </summary>
    Register = 2,
    /// <summary>
    /// Unregister request
    /// </summary>
    Unregister = 3,
    /// <summary>
    /// Refresh login request
    /// </summary>
    RefreshLogin = 4,
}