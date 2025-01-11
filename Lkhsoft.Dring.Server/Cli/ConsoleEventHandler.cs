namespace Lkhsoft.Dring.Server.Cli;

/// <summary>
/// Console event handler
/// </summary>
public class ConsoleEventHandler
{
    /// <summary>
    /// Is exit requested ?
    /// </summary>
    private static bool _exitRequested;

    /// <summary>
    /// Setup console event handlers
    /// </summary>
    public static void SetupConsoleEventHandlers()
    {
        // Désactiver le traitement par défaut de Ctrl+C
        Console.CancelKeyPress += (sender, args) => args.Cancel = true;
    }
    
    /// <summary>
    /// Read a line from the console
    /// </summary>
    public static string ReadLine()
    {
        var input = string.Empty;
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.X)
                {
                    _exitRequested = true;
                    return null;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                    Console.Write("\b \b"); // Effacer le caractère précédent
                }
                else
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
            Thread.Sleep(100);
        }
        Console.WriteLine();
        return input;
    }


    /// <summary>
    /// On cancel key press event handler exit the current context
    /// </summary>
    /// <param name="sender">Sender of the event</param>
    /// <param name="args">Console cancel event arguments</param>
    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
    {
        _exitRequested = true;
        args.Cancel = true; // Annule l'événement pour éviter la fermeture de l'application
    }

    /// <summary>
    /// Is exit requested ?
    /// </summary>
    /// <returns></returns>
    public static bool IsExitRequested()
    {
        return _exitRequested;
    }

    /// <summary>
    /// Reset exit requested to false
    /// </summary>
    public static void ResetExitRequested()
    {
        _exitRequested = false;
    }
}