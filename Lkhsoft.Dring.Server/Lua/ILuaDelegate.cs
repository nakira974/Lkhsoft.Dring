namespace Lkhsoft.Dring.Server.Lua;

/// <summary>
///     Definition of a delegate that can be executed in Lua
/// </summary>
public interface ILuaDelegate
{
    /// <summary>
    ///     Execute the delegate in the Lua environment
    /// </summary>
    /// <param name="luaState">Lua context</param>
    void Execute(NLua.Lua luaState);
}