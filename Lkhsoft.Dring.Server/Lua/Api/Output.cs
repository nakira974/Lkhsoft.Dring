#region

using System.ComponentModel.Composition;
using Lkhsoft.Dring.Shared.Lua;

#endregion

namespace Lkhsoft.Dring.Server.Lua.Api;

/// <summary>
///     Lua print function
/// </summary>
[Export(typeof(ILuaDelegate))]
public class Print : ILuaDelegate
{
    /// <inheritdoc />
    public void Execute(NLua.Lua luaState)
    {
        luaState["PrintMessage"] = new Action<string>(message =>
        {
            Console.WriteLine($"Message from Lua: {message}");
        });
    }
}