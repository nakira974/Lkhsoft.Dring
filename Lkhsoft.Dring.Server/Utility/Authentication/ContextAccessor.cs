using System.ComponentModel.Composition;
using System.Security;
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
    [Import]
    private ISessionService _sessionService { get; set; }

    /// <summary>
    ///     Current user session
    /// </summary>
    private Session _session;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ContextAccessor()
    {
        _sessionService = DefaultContainer.Get<ISessionService>() ??
                          throw new SecurityException("Global session service not found");
    }

    ///<inheritdoc />
    public Session GetSession()
    {
        return _session ?? new Session("guest");
    }

    ///<inheritdoc />
    public void Register(Session @new)
    {
        _session = @new;
        _sessionService.AddSession(@new);
    }

    ///<inheritdoc />
    public void Unregister()
    {
        _sessionService.RemoveSession(_session);
    }

    ///<inheritdoc />
    public async Task<string?> GetRole()
    {
        return await _sessionService.GetUserRoleAsync(GetSession().Username);
    }
}