namespace Lkhsoft.Dring.Server.Lua;

public interface ILuaDelegate
{
    /// <summary>
    /// Execute the delegate in the Lua environment
    /// </summary>
    /// <param name="luaState">Lua context</param>
    void Execute(NLua.Lua luaState);
}