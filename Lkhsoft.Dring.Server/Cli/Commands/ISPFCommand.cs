using System.ComponentModel.Composition;
using System.Configuration;
using Lkhsoft.Dring.Server.Cli.Commands.Authentication;
using Lkhsoft.Dring.Server.Lua.Batch;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
///     ISPF command implementation
/// </summary>
[AuthorizedCommand(AuthorizationType.User, AuthorizationType.Admin)]
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "ISPF")]
[ExportMetadata("CommandAlias", "")]
public class ISPFCommand : CommandBase
{
    /// <summary>
    /// Batch configuration loader
    /// </summary>
    [Import]
    private BatchConfigLoader BatchConfigLoader { get; set; }
    
    /// <inheritdoc />
    public override void Execute(params string[] args)
    {
        Console.WriteLine("ISPF command executed. Entering ISPF environment.");
        ShowISPFMenu();
    }

    /// <summary>
    ///     Display the ISPF menu
    /// </summary>
    private void ShowISPFMenu()
    {
        var exitMenu = false;


        ShowMainMenuOptions();
        
        while (!exitMenu)
            try
            {
                var input = ReadOptions("Select an option: ");

                switch (input?.Trim().ToUpper())
                {
                    case "1":
                        Console.WriteLine("Utilities selected");
                        // Ajoutez ici la logique pour les utilitaires
                        continue;
                    case "2":
                        Console.WriteLine("Foreground selected");
                        // Ajoutez ici la logique pour le foreground
                        continue;
                    case "3":
                        Console.WriteLine("Batch selected");
                        ShowBatchMenu();
                        ShowMainMenuOptions();
                        continue;
                    case "4":
                        Console.WriteLine("TSO selected");
                        // Ajoutez ici la logique pour TSO
                        continue;
                    case "5":
                        Console.WriteLine("Command selected");
                        // Ajoutez ici la logique pour les commandes
                        continue;
                    case "6":
                        Console.WriteLine("Program Management selected");
                        // Ajoutez ici la logique pour la gestion des programmes
                        continue;
                    case "7":
                        Console.WriteLine("System Management selected");
                        // Ajoutez ici la logique pour la gestion du système
                        continue;
                    case "8":
                        Console.WriteLine("Exiting ISPF");
                        exitMenu = true;
                        break;
                    default:
                        ShowMainMenuOptions();
                        Console.WriteLine("Invalid option. Please try again");
                        continue;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
    }
    
    #region BATCH
    /// <summary>
    /// Displays the main menu options
    /// </summary>

    private void ShowMainMenuOptions()
    {
        Console.WriteLine("=========================");
        Console.WriteLine("ISPF Primary Option Menu");
        Console.WriteLine("=========================");
        Console.WriteLine("1. Utilities");
        Console.WriteLine("2. Foreground");
        Console.WriteLine("3. Batch");
        Console.WriteLine("4. TSO");
        Console.WriteLine("5. Command");
        Console.WriteLine("6. Program Management");
        Console.WriteLine("7. System Management");
        Console.WriteLine("8. Exit");
    }

    /// <summary>
    /// Displays the batch menu
    /// </summary>
    /// <param name="backToMenu">Does the user came back to the main menu if canceled ?</param>
    private void ShowBatchMenu()
    {
        var exitBatchMenu = false;
        
        Console.WriteLine();
        Console.WriteLine("===========");
        Console.WriteLine("Batch Menu");
        Console.WriteLine("===========");
        Console.WriteLine("Select a batch job to configure: ");

        // Liste des batchs disponibles
        var batchList = BatchConfigLoader.BatchConfig.Batches.ToList();
        for (ushort i = 0; i < batchList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {batchList[i].Name} - {batchList[i].Description}");
        }

        while (!exitBatchMenu)
        {
            string input;
            try
            {
                input = ReadOptions("Select an option (1 to " + batchList.Count + "): ");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            
            if (UInt16.TryParse(input, out var batchIndex) && batchIndex > 0 && batchIndex <= batchList.Count)
            {
                var selectedBatch = batchList[batchIndex - 1];
                Console.WriteLine($"Selected Batch: {selectedBatch.Name}");
                ConfigureBatchParameters(selectedBatch);
                exitBatchMenu = true; // Quitter le menu après la configuration
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again");
            }
        }
    }

    /// <summary>
    /// Cofnigure the batch parameters
    /// </summary>
    /// <param name="batch">Batch to be configured</param>
    private void ConfigureBatchParameters(BatchScript batch)
    {
        Console.WriteLine("Configuring Batch: " + batch.Name);
        foreach (var param in batch.Parameters)
        {
            Console.WriteLine($"Parameter: {param.Name} ({param.Type}) - Default: {param.DefaultValue}");
            string userInput;
            try
            {
                userInput = ReadOptions($"Enter value for {param.Name} (default: {param.DefaultValue}): ");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            // Si l'utilisateur entre une valeur, l'utiliser, sinon garder la valeur par défaut
            var finalValue = String.IsNullOrWhiteSpace(userInput) ? param.DefaultValue : userInput;
            Console.WriteLine($"{param.Name} set to: {finalValue}");

            // Sauvegarde des paramètres si nécessaire, par exemple dans un dictionnaire ou autre structure
            // batchParameters[param.Name] = finalValue;
        }

        // Une fois les paramètres configurés, planifier ou exécuter le batch
        ScheduleBatch(batch);
    }

    /// <summary>
    /// Schedule the batch job
    /// </summary>
    /// <param name="batch">Batch to be scheduled</param>
    private async void ScheduleBatch(BatchScript batch)
    {
        Console.WriteLine($"Scheduling batch job {batch.Name}...");

        // Créer un dictionnaire qui mappe le nom du batch à son horaire de lancement
        var defaultScheduledTime = ConfigurationManager.AppSettings["BatchDefaultLaunchTime"] ?? "1";
        _ = UInt16.TryParse(defaultScheduledTime, out var defaultLaunchTime);
        var batchSchedule = new Dictionary<string, DateTime>
        {
            { batch.Name, DateTime.Now.AddMinutes(defaultLaunchTime) }
        };

        // Charger la configuration des batchs

        // Créer le ThreadPool et distribuer les batchs
        var batchExecutorPool = new BatchExecutorPool(BatchConfigLoader.BatchConfig.Batches, batchSchedule);

        // S'abonner à l'observable pour afficher les messages dans la console
        batchExecutorPool.CompletionObservable.Subscribe(message =>
        {
            _logger.LogDebug(message);
        });
        
        // Attendre que tous les batchs soient exécutés
        await batchExecutorPool.WaitForCompletionAsync();
    }
    #endregion
}