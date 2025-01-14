using System.ComponentModel.Composition;
using System.Security;
using Lkhsoft.Dring.Server.Cli;
using Lkhsoft.Dring.Server.Utility.Core;

namespace Lkhsoft.Dring.Server.Utility.Authentication;

/// <summary>
///     Context accessor implementation
/// </summary>
[Export(typeof(IContextAccessor))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ContextAccessor : IContextAccessor
{
    /// <summary>
    ///     Global session service
    /// </summary>
    private readonly ISessionService _sessionService;

    /// <summary>
    ///     Current user session
    /// </summary>
    private Session _session;

    /// <summary>
    ///     Current CLI session
    /// </summary>
    private CommandParser CliSession { get; set; }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ContextAccessor()
    {
        _sessionService = DefaultContainer.Get<ISessionService>() ??
                          throw new SecurityException("Global session service not found");
        if (_session is null)
            Register(new Session("guest"));
    }

    ///<inheritdoc />
    public void RegisterCliSession(CommandParser session)
    {
        CliSession = session;
    }

    ///<inheritdoc />
    public CommandParser GetCliSession()
    {
        return CliSession;
    }

    ///<inheritdoc />
    public void Register(Session @new)
    {
        if (_session is not null)
        {
            _session.End();
            _sessionService.RemoveSession(_session);
        }

        _session = @new;
        _session.Start();
        _sessionService.AddSession(@new);
    }

    ///<inheritdoc />
    public Session GetSession()
    {
        return _session;
    }

    ///<inheritdoc />
    public void Unregister()
    {
        _session.End();
        _sessionService.RemoveSession(_session);
        _session = new Session("guest");
    }

    ///<inheritdoc />
    public async Task<string?> GetRole()
    {
        return await _sessionService.GetUserRoleAsync(GetSession().Username);
    }
}