using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Lkhsoft.Dring.Server.Lua.Batch;

/// <summary>
/// LUA batch script executor
/// </summary>
public class BatchExecutorPool
{
    /// <summary>
    /// Maximum number of batch scripts per task
    /// </summary>
    private const int MaxBatchsPerTask = 5;
    
    /// <summary>
    /// Running tasks that contain batch scripts
    /// </summary>
    private List<Task> _tasks = new List<Task>();
    
    /// <summary>
    /// Completion subject
    /// </summary>
    private readonly Subject<string> _completionSubject = new Subject<string>();

    /// <summary>
    /// Completion observable to be notified when all batch scripts are executed
    /// </summary>
    public IObservable<string> CompletionObservable => _completionSubject.AsObservable();

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="batchScripts">Batch scripts to be exeucted</param>
    /// <param name="batchSchedule">Scheduled batch scripts</param>
    public BatchExecutorPool(IEnumerable<BatchScript> batchScripts, Dictionary<string, DateTime> batchSchedule)
    {
        // Regrouper les batchs en groupes de taille maximale
        var batchGroups = batchScripts
            .Select((batch, index) => new { batch, index })
            .GroupBy(x => x.index / MaxBatchsPerTask)
            .Select(group => group.Select(x => x.batch).ToList())
            .ToList();

        // Créer et lancer des tâches pour chaque groupe de batchs
        foreach (var group in batchGroups)
        {
            var executor = new BatchExecutor(group, batchSchedule);
            var task = Task.Run(() => executor.ExecuteBatchs(_completionSubject));
            _tasks.Add(task);
        }
    }

    /// <summary>
    /// Waits for all batch execution tasks to complete
    /// </summary>
    /// <returns>Completed task</returns>
    public async Task WaitForCompletionAsync()
    {
        // Attendre que toutes les tâches se terminent sans bloquer
        await Task.WhenAll(_tasks);
        _completionSubject.OnCompleted();
    }
}