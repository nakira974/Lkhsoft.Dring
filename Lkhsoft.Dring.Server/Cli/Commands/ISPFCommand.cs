using System.ComponentModel.Composition;

namespace Lkhsoft.Dring.Server.Cli.Commands;

/// <summary>
/// ISPF command implementation
/// </summary>
[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "ISPF")]
public class ISPFCommand : ICommand
{
    ///<inheritdoc/>
    public void Execute(params string[] args)
    {
        Console.WriteLine("ISPF command executed. Entering ISPF environment.");
        ShowISPFMenu();
    }

    /// <summary>
    /// Display the ISPF menu
    /// </summary>
    private void ShowISPFMenu()
    {
        bool exitMenu = false;

        while (!exitMenu)
        {
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
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            switch (input?.Trim().ToUpper())
            {
                case "1":
                    Console.WriteLine("Utilities selected.");
                    // Ajoutez ici la logique pour les utilitaires
                    break;
                case "2":
                    Console.WriteLine("Foreground selected.");
                    // Ajoutez ici la logique pour le foreground
                    break;
                case "3":
                    Console.WriteLine("Batch selected.");
                    
                    break;
                case "4":
                    Console.WriteLine("TSO selected.");
                    
                    break;
                case "5":
                    Console.WriteLine("Command selected.");
                    
                    break;
                case "6":
                    Console.WriteLine("Program Management selected.");
                    
                    break;
                case "7":
                    Console.WriteLine("System Management selected.");
                    
                    break;
                case "8":
                    Console.WriteLine("Exiting ISPF.");
                    exitMenu = true;
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}