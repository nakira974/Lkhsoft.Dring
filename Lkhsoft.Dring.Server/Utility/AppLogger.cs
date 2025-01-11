using System.ComponentModel.Composition;
using NLog;

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
/// Logging methods implementation
/// </summary>
[Export(typeof(IAppLogger))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AppLogger : IAppLogger
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Default constructor
    /// </summary>
    public AppLogger()
    {
        NLogConfigurator.ConfigureFromYaml("server.yaml");
    }

    ///<inheritdoc/>
    public void LogInfo(string message)
    {
        _logger.Info(message);
    }

    ///<inheritdoc/>
    public void LogError(string message)
    {
        _logger.Error(message);
    }

    ///<inheritdoc/>
    public void LogWarning(string message)
    {
        _logger.Warn(message);
    }
}