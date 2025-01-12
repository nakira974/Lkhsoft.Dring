using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Lkhsoft.Dring.Server.Utility;

/// <summary>
///     Console event handler
/// </summary>
public class ConsoleEventHandler
{
    /// <summary>
    ///     Is exit requested ?
    /// </summary>
    private static bool _exitRequested;

    /// <summary>
    /// Commands history
    /// </summary>
    private static LinkedList<string> _history = new LinkedList<string>();

    /// <summary>
    /// Current history index
    /// </summary>
    private static LinkedListNode<string> _currentHistoryNode = null;

    /// <summary>
    /// Map of AES keys by version number
    /// </summary>
    private static readonly Dictionary<int, byte[]> KeyVersions = new Dictionary<int, byte[]>
    {
        {1, Convert.FromBase64String("9WpFqL8J7g5dYq8B5jK9nl6jPjdMN1FobNfhz0axdkM=")},
        {2, Convert.FromBase64String("tDF3v6G3PqNbIh7D8HSTGpN9oYfrXfH76nboydHpCeY=")}
    };

    private static readonly byte[] FixedIV = Encoding.UTF8.GetBytes("0123456789abcdef");

    /// <summary>
    /// Current key version
    /// </summary>
    private static readonly int CurrentKeyVersion = 2;

    /// <summary>
    ///     Setup console event handlers
    /// </summary>
    public static void SetupConsoleEventHandlers()
    {
        // Désactiver le traitement par défaut de Ctrl+C
        Console.CancelKeyPress += (sender, args) => args.Cancel = true;
    }


    /// <summary>
    /// Read a line from the console with command history support.
    /// </summary>
    public static string ReadLine()
    {
        var input = string.Empty;
        _currentHistoryNode = null; // Réinitialise la position dans l'historique

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                // Gestion de la sortie (Ctrl+X)
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.X)
                {
                    return null;
                }

                // Gestion de la validation (Entrée)
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        _history.AddLast(input); // Ajouter la saisie à l'historique
                    }

                    return input;
                }

                // Gestion de la flèche haut (historique précédent)
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (_currentHistoryNode == null)
                    {
                        _currentHistoryNode = _history.Last; // Se positionner au dernier élément
                    }
                    else if (_currentHistoryNode.Previous != null)
                    {
                        _currentHistoryNode = _currentHistoryNode.Previous; // Remonter
                    }

                    if (_currentHistoryNode != null)
                    {
                        ClearCurrentInput(input);
                        input = _currentHistoryNode.Value;
                        Console.Write(input);
                    }

                    continue;
                }

                // Gestion de la flèche bas (historique suivant)
                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (_currentHistoryNode != null && _currentHistoryNode.Next != null)
                    {
                        _currentHistoryNode = _currentHistoryNode.Next; // Descendre
                    }
                    else
                    {
                        _currentHistoryNode = null; // Fin de l'historique
                    }

                    ClearCurrentInput(input);
                    input = _currentHistoryNode?.Value ?? string.Empty;
                    Console.Write(input);

                    continue;
                }

                // Gestion de la touche Backspace
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                    Console.Write("\b \b"); // Effacer le dernier caractère
                    continue;
                }

                // Gestion des caractères normaux
                if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }

            Thread.Sleep(100);
        }
    }


    /// <summary>
    /// Efface l'entrée actuelle de la console.
    /// </summary>
    /// <param name="input">L'entrée actuelle à effacer.</param>
    private static void ClearCurrentInput(string input)
    {
        // Efface chaque caractère affiché dans la console
        for (int i = 0; i < input.Length; i++)
        {
            Console.Write("\b \b");
        }
    }

    /// <summary>
    /// Reads a line from the console, encrypts it using the current AES key, and returns the encrypted string
    /// </summary>
    public static string? ReadAndEncryptSecureLine(string iv = "")
    {
        Console.WriteLine("Enter sensitive input (Ctrl+X to cancel):");

        // Lire la saisie utilisateur sous forme de SecureString
        var secureInput = ReadSecureLine();

        if (secureInput is null)
        {
            Console.WriteLine("Input cancelled.");
            return null;
        }

        // Chiffrer les données avec la clé active
        var encryptedData = EncryptWithVersion(secureInput, CurrentKeyVersion, Encoding.UTF8.GetBytes(iv));

        return encryptedData;
    }

    /// <summary>
    /// Reads a secure line from the console and returns it as a SecureString.
    /// </summary>
    private static SecureString? ReadSecureLine()
    {
        var secureInput = new SecureString();

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                // Détecter Ctrl+X pour annuler
                if (key.Modifiers is ConsoleModifiers.Control && key.Key is ConsoleKey.X)
                {
                    Console.WriteLine();
                    return null;
                }

                // Touche Entrée pour terminer la saisie
                if (key.Key is ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                // Gestion de la touche Backspace
                if (key.Key is ConsoleKey.Backspace && secureInput.Length > 0)
                {
                    secureInput.RemoveAt(secureInput.Length - 1);
                    Console.Write("\b \b"); // Efface visuellement le caractère précédent
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    secureInput.AppendChar(key.KeyChar);
                    Console.Write('*'); // Masque visuellement la saisie
                }
            }

            System.Threading.Thread.Sleep(100);
        }

        secureInput.MakeReadOnly();
        return secureInput;
    }

    /// <summary>
    /// Encrypts a SecureString using AES-256 with versioning support
    /// </summary>
    private static string? EncryptWithVersion(SecureString? secureString, int version, byte[]? iv = null)
    {
        if (!KeyVersions.TryGetValue(version, out var key))
            throw new ArgumentException("Invalid key version");

        var secureStringPointer = IntPtr.Zero;

        try
        {
            // Convertir le SecureString en texte clair temporairement
            secureStringPointer =
                Marshal.SecureStringToGlobalAllocUnicode(secureString ??
                                                         throw new ArgumentNullException(nameof(secureString)));
            var plainText = Marshal.PtrToStringUni(secureStringPointer);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv ?? FixedIV; // Utiliser un IV fixe
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var memoryStream = new MemoryStream();
            memoryStream.WriteByte((byte) version); // Écrit la version au début
            memoryStream.Write(aes.IV, 0, aes.IV.Length); // Écrit l'IV

            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
            {
                writer.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        finally
        {
            if (secureStringPointer != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(secureStringPointer); // Nettoie les données sensibles
        }
    }


    /// <summary>
    ///     On cancel key press event handler exit the current context
    /// </summary>
    /// <param name="sender">Sender of the event</param>
    /// <param name="args">Console cancel event arguments</param>
    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
    {
        _exitRequested = true;
        args.Cancel = true; // Annule l'événement pour éviter la fermeture de l'application
    }

    /// <summary>
    ///     Is exit requested ?
    /// </summary>
    /// <returns></returns>
    public static bool IsExitRequested()
    {
        return _exitRequested;
    }

    /// <summary>
    ///     Reset exit requested to false
    /// </summary>
    public static void ResetExitRequested()
    {
        _exitRequested = false;
    }
}