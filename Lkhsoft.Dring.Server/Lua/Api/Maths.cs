using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Lua.Api;

/// <summary>
///     Lua math function
/// </summary>
[Export(typeof(ILuaDelegate))]
public class AddFunction : ILuaDelegate
{
    /// <inheritdoc />
    public void Execute(NLua.Lua luaState)
    {
        luaState["Add"] = new Func<double, double, double>((a, b) => a + b);
    }
}