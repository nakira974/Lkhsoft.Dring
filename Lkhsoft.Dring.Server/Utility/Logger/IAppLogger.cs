namespace Lkhsoft.Dring.Server.Utility.Logger;

/// <summary>
///     Logging methods definition
/// </summary>
public interface IAppLogger
{
    /// <summary>
    ///     Logging an information message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogInfo(string message);

    /// <summary>
    ///     Logging an error message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogError(string message);

    /// <summary>
    ///     Logging a warning message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogWarning(string message);

    /// <summary>
    ///     Logging a debug message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogDebug(string message);

    /// <summary>
    ///     Logging a fatal message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogFatal(string message);

    /// <summary>
    ///     Logging a trace message
    /// </summary>
    /// <param name="message">Message to log</param>
    void LogTrace(string message);
}