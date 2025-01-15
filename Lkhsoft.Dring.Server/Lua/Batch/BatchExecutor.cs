#region

using System.Configuration;
using System.Globalization;
using System.Reactive.Subjects;
using Lkhsoft.Dring.Shared.Core;

#endregion

namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// LUA batch script executor
/// </summary>
public class BatchExecutor
{
    /// <summary>
    /// Batch scripts to be executed
    /// </summary>
    private IEnumerable<BatchScript> _batchScripts;

    /// <summary>
    /// Scheduled batch execution times
    /// </summary>
    private Dictionary<string, DateTime> _batchSchedule;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="batchScripts">Batch scripts to be executed</param>
    /// <param name="batchSchedule">Scheduled batch scripts</param>
    public BatchExecutor(IEnumerable<BatchScript> batchScripts, Dictionary<string, DateTime> batchSchedule)
    {
        _batchScripts = batchScripts;
        _batchSchedule = batchSchedule;
    }

    /// <summary>
    /// Launch the batch scripts execution
    /// </summary>
    public void ExecuteBatchs(Subject<string> completionSubject)
    {
        // Lancer une tâche pour chaque batch à la date de lancement spécifiée
        foreach (var batch in _batchSchedule)
        {
            var batchScript = _batchScripts.FirstOrDefault(b => b.Name == batch.Key);
            if (batchScript != null) ScheduleBatchExecution(batchScript, batch.Value, completionSubject);
        }
    }

    /// <summary>
    /// Schedule the batch execution
    /// </summary>
    /// <param name="batchScript">Batch script to be executed</param>
    /// <param name="scheduledTime">Execution date time</param>
    private void ScheduleBatchExecution(BatchScript batchScript, DateTime scheduledTime,
        Subject<string> completionSubject)
    {
        var delay = scheduledTime - DateTime.Now;
        if (delay > TimeSpan.Zero)
            // Si l'exécution doit être différée, planifier une tâche après le délai
            Task.Delay(delay).ContinueWith(_ => { ExecuteLuaScript(batchScript, completionSubject); });
        else
            // Si l'heure de lancement est déjà passée, exécuter immédiatement
            ExecuteLuaScript(batchScript, completionSubject);
    }

    /// <summary>
    /// Execute the specified Lua script
    /// </summary>
    /// <param name="script">Lua script to be executed</param>
    private void ExecuteLuaScript(BatchScript script, Subject<string> completionSubject)
    {
        var configLoader = DefaultContainer.Get<BatchConfigLoader>();
        var luaScriptPath = string.Empty;

        if (OperatingSystem.IsWindows())
        {
            luaScriptPath = string.Format(CultureInfo.CurrentCulture, "{0}\\{1}", configLoader?.BatchConfig.Path,
                script.Script);
        }
        else if (OperatingSystem.IsLinux())
        {
            luaScriptPath = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", configLoader?.BatchConfig.Path,
                script.Script);
        }
        else
        {
            Console.WriteLine("Unsupported operating system for Lua batch script execution.");
            return;
        }

        if (string.IsNullOrWhiteSpace(luaScriptPath) || !File.Exists(luaScriptPath))
        {
            Console.WriteLine($"Lua script {script} not found");
            return;
        }

        var luaScript = File.ReadAllText(luaScriptPath);

        try
        {
            using var lua = new NLua.Lua();

            lua.NewTable("arg");

            var outputAllowedConfig = ConfigurationManager.AppSettings["BatchOutputAllowed"] ?? "False";

            _ = bool.TryParse(outputAllowedConfig, out var outputAllowed);

            if (!outputAllowed)
                // Ne pas afficher la sortie standard de lua pour les batches
                lua.DoString("print = function() end");

            for (byte i = 0; i < script.Parameters.Count(); i++)
                lua[$"arg[{i}]"] = script.Parameters.ElementAt(i).DefaultValue;

            lua.DoString(luaScript);
            completionSubject.OnNext($"Lua script {script} executed successfully.");
        }
        catch (Exception ex)
        {
            completionSubject.OnNext($"Error executing Lua script {script}: {ex.Message}");
        }
    }
}