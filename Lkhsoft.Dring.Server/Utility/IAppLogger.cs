namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
/// Logging methods definition
/// </summary>
public interface IAppLogger
{
    /// <summary>
    /// Logging an information message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogInfo(string message);
    
    /// <summary>
    /// Logging an error message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogError(string message);
    
    /// <summary>
    /// Logging a warning message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogWarning(string message);
}