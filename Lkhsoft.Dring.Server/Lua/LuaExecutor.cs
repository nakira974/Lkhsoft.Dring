using System.Composition;
using Lkhsoft.Dring.Server.Utility.Authentication;

namespace Lkhsoft.Dring.Server.Lua;

/// <summary>
///     Lua script executor
/// </summary>
[Export(typeof(LuaExecutor))]
public class LuaExecutor
{
    /// <summary>
    ///     Lua state
    /// </summary>
    private readonly NLua.Lua _luaState;

    /// <summary>
    ///     Lua delegates
    /// </summary>
    private readonly IEnumerable<ILuaDelegate> _luaDelegates;
    
    /// <summary>
    ///     Current context accessor
    /// </summary>
    private readonly IContextAccessor _contextAccessor;

    /// <summary>
    ///     Default constructor
    /// </summary>
    [ImportingConstructor]
    public LuaExecutor([Import] IContextAccessor contextAccessor,
        [ImportMany] IEnumerable<ILuaDelegate> luaDelegates
        )
    {
        _contextAccessor = contextAccessor;
        _luaDelegates = luaDelegates;
        _luaState = new NLua.Lua();
        RegisterFunctions();
    }


    /// <summary>
    ///     Registers discovered C# functions to Lua environment
    /// </summary>
    private void RegisterFunctions()
    {
        foreach (var function in
                 _luaDelegates) function.Execute(_luaState); // Enregistrer chaque fonction dans l'environnement Lua
    }

    /// <summary>
    ///     Runs a Lua script
    /// </summary>
    /// <param name="script">Script to execute</param>
    public void ExecuteScript(string script)
    {
        Console.SetOut(_contextAccessor.GetTextWriter());
        try
        {
            Console.WriteLine("Exécution du script Lua...");
            var result = _luaState.DoString(script);
            if (result != null)
            {
                Console.WriteLine("Résultats :");
                foreach (var res in result) Console.WriteLine(res);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'exécution du script Lua : {ex.Message}");
        }
    }

    /// <summary>
    ///     Close Lua state
    /// </summary>
    public void Close()
    {
        _luaState.Dispose();
        Console.WriteLine("Etat Lua fermé.");
    }
}