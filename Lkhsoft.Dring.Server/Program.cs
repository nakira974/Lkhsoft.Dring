#region

using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SQLite;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Lkhsoft.Dring.Messages;
using Lkhsoft.Dring.Server.Utility;
using Lkhsoft.Dring.Server.Utility.Core;
using Lkhsoft.Dring.Shared.Cli;
using Lkhsoft.Dring.Shared.Core;
using Lkhsoft.Dring.Shared.Core.Authentication;
using Lkhsoft.Dring.Shared.Core.Logger;

#endregion

namespace Lkhsoft.Dring.Server;

/// <summary>
///     Main class of the server
/// </summary>
internal class Program
{
    /// <summary>
    ///     Default TCP port
    /// </summary>
    private const ushort defaultTcpPort = 12345;

    /// <summary>
    ///     Default UDP port
    /// </summary>
    private const ushort defaultUdpPort = 54321;

    /// <summary>
    ///     Logger
    /// </summary>
    private static IAppLogger _logger;

    /// <summary>
    ///     Tcp port from configuration file
    /// </summary>
    private static readonly ushort TcpPort = GetTcpPort();

    /// <summary>
    ///     Udp port from configuration file
    /// </summary>
    private static readonly ushort UdpPort = GetUdpPort();

    /// <summary>
    ///     Mutex for the server
    /// </summary>
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    ///     Mutex for CLI
    /// </summary>
    private static readonly Semaphore _cliSemaphore = new(1, 1);

    /// <summary>
    ///     Tcp channels for each client
    /// </summary>
    private static readonly ConcurrentDictionary<ConnectedUser, SslStream> Clients = new();

    /// <summary>
    ///     Message priority queue
    /// </summary>
    private static readonly PriorityQueue<Message, byte> GlobalPriorityQueue = new();

    /// <summary>
    ///     Server certificate
    /// </summary>
    private static readonly X509Certificate2? ServerCertificate = LoadCertificate();

    /// <summary>
    ///     Server text writer
    /// </summary>
    private static ServerTextWriter _serverTextWriter;

    /// <summary>
    ///     Main server task executing CLI and network tasks
    /// </summary>
    private static async Task Main(string[] args)
    {
        var originalConsoleOut = Console.Out;
        _serverTextWriter = new ServerTextWriter(originalConsoleOut, Encoding.UTF8);
        Console.SetOut(originalConsoleOut);

        Console.WriteLine("Server is starting...");
        _logger = DefaultContainer.Get<IAppLogger>() ??
                  throw new InvalidOperationException("Could not load app logger");

        var initializer = new DatabaseInitializer();
        await initializer.InitializeDatabaseAsync();
        _logger.LogDebug("Database has been initialized successfully");
        
        var cts = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveTcp(cts.Token), cts.Token);
        _ = Task.Run(() => ReceiveUdp(cts.Token), cts.Token);

        await Task.Delay(100, cts.Token);
        await _semaphore.WaitAsync(cts.Token);
        _semaphore.Release();
        Console.WriteLine("CLI Ready. Type 'exit' to quit.");
        _logger.LogInfo($"Server started on ports TCP:{TcpPort} and UDP:{UdpPort}");

        var commandParser = new CommandParser(originalConsoleOut);
        SetupSignalHandlers(cts);

        while (true)
            if (Console.KeyAvailable)
            {
                Console.Write("dring/guest > ");
                var input = ConsoleEventHandler.ReadLine();
                _logger.LogInfo($"Received command: {input ?? "EXIT"}");
                if (input is null)
                {
                    commandParser.ParseAndExecute("EXIT");
                    break;
                }

                commandParser.ParseAndExecute(input);
            }

        Console.WriteLine("Shutting down...");
    }

    /// <summary>
    /// User CLI session
    /// </summary>
    /// <param name="commandParser">User shell session</param>
    public static Task RunSession(CommandParser commandParser)
    {
        while (true)
        {
            Console.Write($"dring/{commandParser.GetUserSession().Username} > ");
            var input = ConsoleEventHandler.ReadLine();
            _logger.LogInfo($"Received command: {input ?? "EXIT"}");
            if (input is null || input.ToUpper(CultureInfo.CurrentCulture) is "LOGOFF")
            {
                commandParser.ParseAndExecute("LOGOFF");
                break;
            }

            commandParser.ParseAndExecute(input);
        }

        Console.WriteLine("Shutting down session...");
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Receives messages from TCP clients
    /// </summary>
    private static async Task ReceiveTcp(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        var listener = new TcpListener(IPAddress.Any, TcpPort);
        listener.Start();
        Console.WriteLine($"TCP listener started on port {TcpPort}.");

        _semaphore.Release();

        while (cancellationToken.IsCancellationRequested == false)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken);
        
            _ = Task.Run(() => HandleClient(client, cancellationToken), cancellationToken);
        }
    }
    
    /// <summary>
    ///     Handles a client connection
    /// </summary>
    private static async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
       try
       {
           var stream = client.GetStream();
           var sslStream = new SslStream(client.GetStream(), true);
           await sslStream.AuthenticateAsServerAsync(
               ServerCertificate ?? throw new InvalidOperationException("Server certificate is null"),
               false,
               SslProtocols.Tls12,
               false);


           // Authenticate client
           var connectionAttempt = 1;
           ConnectedUser? connectedUser = null;
           while (client.Connected && connectedUser is null)
           {
               connectionAttempt++;
               var authDataBuffer = new byte[1024];
               var read = await sslStream.ReadAsync(authDataBuffer, 0, authDataBuffer.Length, cancellationToken);
               if (connectionAttempt > 3) break;
               Array.Resize(ref authDataBuffer, read);
               var memoryStream = new MemoryStream(authDataBuffer);
               connectedUser = await AuthenticateClient(memoryStream, sslStream, cancellationToken);
           }

           if (connectionAttempt > 3)
           {
               var unauthorizedResponse = new Message(MessageType.Authentication, [0x3], 0, 1);
               var unauthorizedResponseStream = new MemoryStream();
               await JsonSerializer.SerializeAsync<Message>(unauthorizedResponseStream, unauthorizedResponse, cancellationToken: cancellationToken);
               await sslStream.WriteAsync(unauthorizedResponseStream.ToArray(), cancellationToken);
           }
            
           if (connectedUser is null || !client.Connected || connectionAttempt >= 3)
           {
               if(!client.Connected)
                   throw new AuthenticationException("Client déconnecté");
                
               if(connectionAttempt >= 3)
                   throw new AuthenticationException("Nombre de tentatives d'authentification dépassé");
                
               throw new AuthenticationException("Client non authentifié");
           }

           Console.WriteLine($"Client connected: {connectedUser}");
           Clients.TryAdd(connectedUser, sslStream);

           _ = Job(connectedUser, stream, cancellationToken);
       }
       catch (Exception ex)
       {
           var errorMessage = ex is AuthenticationException ? $"Erreur d'authentication: {ex.Message}" : $"Erreur dans ReceiveTcp: {ex.Message}";
           Console.WriteLine(errorMessage);
           _logger.LogInfo(errorMessage);
           client.Close();
       }
    }

    /// <summary>
    ///     Receives messages from UDP clients
    /// </summary>
    private static async Task ReceiveUdp(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        var udpListener = new UdpClient(UdpPort);
        Console.WriteLine($"UDP listener started on port {UdpPort}.");

        _semaphore.Release();
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Any, 0);
                var result = udpListener.Receive(ref endpoint);
                var message = Encoding.UTF8.GetString(result);
                Console.WriteLine($"UDP message received: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveUdp loop: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Processes messages for a specific client
    /// </summary>
    /// <param name="connectedUser">The ID of the client</param>
    /// <param name="stream">The client's stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private static async Task Job(ConnectedUser? connectedUser, NetworkStream stream, CancellationToken cancellationToken)
    {
        while (stream.CanRead)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Client disconnected: {connectedUser?.UserName}");
                    Clients.TryRemove(connectedUser ?? throw new ArgumentNullException(nameof(connectedUser)), out _);
                    break;
                }

                Array.Resize(ref buffer, bytesRead);
                var message = new Message(MessageType.Content, buffer, 0, bytesRead);
                lock (GlobalPriorityQueue)
                {
                    GlobalPriorityQueue.Enqueue(message, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Job loop for client {connectedUser}: {ex.Message}");
                break;
            }
        }
    }

    /// <summary>
    ///     Authenticates a client using a certificate and verifies user credentials
    /// </summary>
    /// <param name="memoryStream">Content send by the client</param>
    /// <param name="sslStream">The client's network stream</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns>The client ID if authentication succeeds, null otherwise</returns>
    private static async Task<ConnectedUser?> AuthenticateClient(MemoryStream memoryStream, SslStream sslStream, CancellationToken cancellationToken)
    {
        var serializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = false
        };
        var deserializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = false
        };
        try
        {
            memoryStream.Position = 0;
            var message = await JsonSerializer.DeserializeAsync<Message>(memoryStream, deserializerOptions, cancellationToken);
            if (message is null) throw new SerializationException("Could not deserialize authentication message");
            memoryStream = new MemoryStream(message.Data ?? throw new InvalidOperationException("Authentication message data is null"));
            var connectedUser = await JsonSerializer.DeserializeAsync<ConnectedUser>(memoryStream, deserializerOptions, cancellationToken);
            if (connectedUser is null) throw new InvalidOperationException("Could not deserialize user");
            var isAuthenticated = await Auth(connectedUser, cancellationToken);
            if (isAuthenticated)
            {
                var okReponse = new Message(MessageType.Authentication, [0x1], 0, 1);
                var okResponseStream = new MemoryStream();
                await JsonSerializer.SerializeAsync(okResponseStream, okReponse, serializerOptions, cancellationToken);
                await sslStream.WriteAsync(okResponseStream.ToArray(), cancellationToken);
                return connectedUser;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error authenticating client: {ex.Message}");
        }

        var unauthorizedResponse = new Message(MessageType.Authentication, [0x0], 0, 1);
        var unauthorizedResponseStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(unauthorizedResponseStream, unauthorizedResponse,
            serializerOptions, cancellationToken);
        await sslStream.WriteAsync(unauthorizedResponseStream.ToArray(), cancellationToken);
        return null;
    }

    /// <summary>
    /// Set up signal handlers for graceful shutdown
    /// </summary>
    private static void SetupSignalHandlers(CancellationTokenSource cancellationTokenSource)
    {
        var isExiting = false;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            HandleShutdown(cancellationTokenSource);
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) => { HandleShutdown(cancellationTokenSource); };
        return;

        async void HandleShutdown(CancellationTokenSource tokenSource)
        {
            try
            {
                // Évite les appels multiples
                if (isExiting) return; 
                isExiting = true;
                await tokenSource.CancelAsync();
                Console.WriteLine("Shutdown signal received. Cleaning up...");
                Console.WriteLine("Server is shutting down gracefully");
                DefaultContainer.Get<IAppLogger>()?.LogTrace("Shutdown signal received, server shut down gracefully");
                DefaultContainer.Get<ISessionService>()?.ClearAllSessions();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in HandleShutdown: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Authenticates a user against the SQLite database
    /// </summary>
    /// <param name="user">The connected username</param>
    /// <returns>True if authentication succeeds, false otherwise</returns>
    private static async Task<bool> Auth(ConnectedUser user, CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ServerDB"].ConnectionString
                                   ?? throw new InvalidOperationException("Datasource connection string not found");

            await using var connection = new SQLiteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
            await using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.UserName);
            command.Parameters.AddWithValue("@Password", user.EncryptedPassword);

            var result = (long) (await command.ExecuteScalarAsync(cancellationToken) ?? throw new SQLiteException("SQL error"));
            return result == 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Auth: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Loads the server certificate from the local store
    /// </summary>
    /// <returns>The loaded X509 certificate</returns>
    private static X509Certificate2? LoadCertificate()
    {
        try
        {
            // Load the certificate from the .pfx file
            // Retrieve the plain-text password from the config file
            var certificatePath = ConfigurationManager.AppSettings["CertificatePath"] ??
                                  throw new InvalidOperationException("Certificate path is missing");
            var passwordPlain = ConfigurationManager.AppSettings["CertificatePassword"] ??
                                throw new InvalidOperationException("Certificate password is missing");
            var secureString = new SecureString();
            foreach (var c in passwordPlain) secureString.AppendChar(c);
            secureString.MakeReadOnly(); // Make the SecureString immutable

            // Convert the plain-text password to SecureString

            // Load the certificate using the secure password
            var certificate = new X509Certificate2(certificatePath, secureString,
                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

            Console.WriteLine("Certificate loaded successfully!");
            return certificate;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading certificate: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    ///     Returns the UDP port from the configuration file
    /// </summary>
    /// <returns></returns>
    private static ushort GetUdpPort()
    {
        var udpPort = ConfigurationManager.AppSettings["UdpPort"] ??
                      throw new InvalidOperationException("Udp port is missing");

        return ushort.TryParse(udpPort, out var port) ? port : defaultUdpPort;
    }

    /// <summary>
    ///     Returns the TCP port from the configuration file
    /// </summary>
    /// <returns></returns>
    private static ushort GetTcpPort()
    {
        var udpPort = ConfigurationManager.AppSettings["TcpPort"] ??
                      throw new InvalidOperationException("Tcp port is missing");

        return ushort.TryParse(udpPort, out var port) ? port : defaultTcpPort;
    }
}