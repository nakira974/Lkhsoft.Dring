using System.ComponentModel.Composition;
using NLog;

namespace Lkhsoft.Dring.Server.Utility.Logger;

/// <summary>
///     Logging methods implementation
/// </summary>
[Export(typeof(IAppLogger))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AppLogger : IAppLogger
{
    /// <summary>
    ///     App logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Default constructor
    /// </summary>
    public AppLogger()
    {
        NLogConfigurator.ConfigureFromYaml("nlog.config.yaml");
    }

    /// <inheritdoc />
    public void LogInfo(string message)
    {
        Logger.Info(message);
    }

    /// <inheritdoc />
    public void LogError(string message)
    {
        Logger.Error(message);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        Logger.Warn(message);
    }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
        Logger.Debug(message);
    }

    /// <inheritdoc />
    public void LogFatal(string message)
    {
        Logger.Fatal(message);
    }

    /// <inheritdoc />
    public void LogTrace(string message)
    {
        Logger.Trace(message);
    }
}