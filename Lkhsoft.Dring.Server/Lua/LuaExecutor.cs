using System.Composition;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server.Lua;

/// <summary>
/// Lua script executor
/// </summary>
[Export(typeof(LuaExecutor))]
public class LuaExecutor
{
    private readonly NLua.Lua _luaState;

    [ImportMany]
    public IEnumerable<ILuaDelegate> LuaDelegates { get; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public LuaExecutor()
    {
        _luaState = new NLua.Lua();
        RegisterFunctions();
    }

    /// <summary>
    /// Registers discovered C# functions to Lua environment
    /// </summary>
    private void RegisterFunctions()
    {
        foreach (var function in LuaDelegates)
        {
            function.Execute(_luaState);  // Enregistrer chaque fonction dans l'environnement Lua
        }
    }

    /// <summary>
    /// Runs a Lua script
    /// </summary>
    /// <param name="script">Script to execute</param>
    public void ExecuteScript(string script)
    {
        try
        {
            Console.WriteLine("Exécution du script Lua...");
            var result = _luaState.DoString(script);
            if (result != null)
            {
                Console.WriteLine("Résultats :");
                foreach (var res in result)
                {
                    Console.WriteLine(res);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'exécution du script Lua : {ex.Message}");
        }
    }

    /// <summary>
    /// Close Lua state
    /// </summary>
    public void Close()
    {
        _luaState.Dispose();
        Console.WriteLine("Etat Lua fermé.");
    }
}