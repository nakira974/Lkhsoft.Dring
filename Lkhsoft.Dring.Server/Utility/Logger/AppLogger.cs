using System.ComponentModel.Composition;
using NLog;

namespace Lkhsoft.Dring.Server.Utility;

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
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

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
        _logger.Info(message);
    }

    /// <inheritdoc />
    public void LogError(string message)
    {
        _logger.Error(message);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        _logger.Warn(message);
    }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }

    /// <inheritdoc />
    public void LogFatal(string message)
    {
        _logger.Fatal(message);
    }

    /// <inheritdoc />
    public void LogTrace(string message)
    {
        _logger.Trace(message);
    }
}