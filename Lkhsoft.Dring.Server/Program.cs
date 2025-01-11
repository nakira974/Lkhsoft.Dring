using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Lkhsoft.Dring.Server.Cli;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server;

/// <summary>
/// Main class of the server
/// </summary>
internal class Program
{
    /// <summary>
    /// Default TCP port
    /// </summary>
    private const ushort DefaultTcpPort = 12345;
    
    /// <summary>
    /// Default UDP port
    /// </summary>
    private const ushort DefaultUdpPort = 54321;
    
    /// <summary>
    /// Tcp port from configuration file
    /// </summary>
    private static readonly ushort TcpPort = GetTcpPort();
    
    /// <summary>
    /// Udp port from configuration file
    /// </summary>
    private static readonly ushort UdpPort = GetUdpPort();
    
    /// <summary>
    /// Mutex for the server
    /// </summary>
    private static SemaphoreSlim _semaphore = new(1, 1);
    
    /// <summary>
    /// Mutex for CLI
    /// </summary>
    private static Semaphore _cliSemaphore = new(1, 1);
    
    /// <summary>
    /// Tcp channels for each client
    /// </summary>
    private static readonly ConcurrentDictionary<string, NetworkStream> Clients = new();
    
    /// <summary>
    /// Send channels for each client
    /// </summary>
    private static readonly ConcurrentDictionary<string, UdpClient?> SendChannels = new();
    
    /// <summary>
    /// Message priority queue
    /// </summary>
    private static readonly PriorityQueue<Message, byte> GlobalPriorityQueue = new();
    
    /// <summary>
    /// Server certificate
    /// </summary>
    private static readonly X509Certificate2? ServerCertificate = LoadCertificate();
    
    /// <summary>
    /// Logger part
    /// </summary>
    [Import] 
    private readonly IAppLogger _logger;
    
    /// <summary>
    /// Main server task executing CLI and network tasks
    /// </summary>
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Server is starting...");
        NLogConfigurator.ConfigureFromYaml("server.yaml");
        
        _ = Task.Run(() => ReceiveTcp());
        _ = Task.Run(() => ReceiveUdp());
        _ = Task.Run(() => Transmit());

        await Task.Delay(100);
        await _semaphore.WaitAsync();
        _semaphore.Release();
        Console.WriteLine("CLI Ready. Type 'exit' to quit.");
        var commandParser = new CommandParser();
        ConsoleEventHandler.SetupConsoleEventHandlers();
        
        while (true)
        {
            Console.Write("instance/admin > ");
            var input = ConsoleEventHandler.ReadLine();
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
    ///     Receives messages from TCP clients
    /// </summary>
    private static async Task ReceiveTcp()
    {
        await _semaphore.WaitAsync();
        var listener = new TcpListener(IPAddress.Any, TcpPort);
        listener.Start();
        Console.WriteLine("TCP listener started on port 12345.");

        _semaphore.Release();
        while (true)
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                var stream = client.GetStream();

                // Authenticate client
                var id = await AuthenticateClient(stream);
                if (string.IsNullOrEmpty(id))
                {
                    client.Close();
                    continue;
                }

                Clients.TryAdd(id, stream);
                SendChannels.TryAdd(id, new UdpClient());

                Console.WriteLine($"Client connected: {id}");

                _ = Job(id, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveTcp loop: {ex.Message}");
            }
    }

    /// <summary>
    ///     Receives messages from UDP clients
    /// </summary>
    private static async Task ReceiveUdp()
    {
        await _semaphore.WaitAsync();
        var udpListener = new UdpClient(UdpPort);
        Console.WriteLine("UDP listener started on port 54321.");

        _semaphore.Release();
        while (true)
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

    /// <summary>
    ///     Processes messages for a specific client
    /// </summary>
    /// <param name="idClient">The ID of the client</param>
    /// <param name="stream">The client's stream</param>
    private static async Task Job(string? idClient, NetworkStream stream)
    {
        while (stream.CanRead)
            try
            {
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Client disconnected: {idClient}");
                    Clients.TryRemove(idClient ?? throw new ArgumentNullException(nameof(idClient)), out _);
                    SendChannels.TryRemove(idClient, out _);
                    break;
                }

                var message = new Message(idClient, buffer, 0, bytesRead, ServerCertificate);
                lock (GlobalPriorityQueue)
                {
                    GlobalPriorityQueue.Enqueue(message, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Job loop for client {idClient}: {ex.Message}");
                break;
            }
    }

    /// <summary>
    ///     Transmits messages via UDP based on the global priority queue
    /// </summary>
    private static async Task Transmit()
    {
        while (true)
            try
            {
                Message message;
                lock (GlobalPriorityQueue)
                {
                    if (GlobalPriorityQueue.Count > 0)
                    {
                        message = GlobalPriorityQueue.Dequeue();
                    }
                    else
                    {
                        Thread.Sleep(100); // Avoid busy-waiting
                        continue;
                    }
                }

                if (SendChannels.TryGetValue(
                        message.ClientId ?? throw new InvalidOperationException("Client id is null"),
                        out var udpClient))
                    if (udpClient != null)
                        await udpClient.SendAsync(message.Data, message.Data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Transmit loop: {ex.Message}");
            }
    }

    /// <summary>
    ///     Authenticates a client using a certificate and verifies user credentials
    /// </summary>
    /// <param name="stream">The client's network stream</param>
    /// <returns>The client ID if authentication succeeds, null otherwise</returns>
    private static async Task<string?> AuthenticateClient(NetworkStream stream)
    {
        try
        {
            var buffer = new byte[256];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                var encryptedData = buffer.AsSpan(0, bytesRead).ToArray();
                using var rsa = (ServerCertificate ?? throw new InvalidOperationException("Invalid certificate"))
                    .GetRSAPrivateKey();
                var decryptedData = rsa?.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
                string?[] authData = Encoding.UTF8.GetString(decryptedData ?? Array.Empty<byte>()).Split(':');

                if (authData.Length == 2)
                {
                    var user = authData[0];
                    var password = new SecureString();
                    foreach (var c in authData[1]!) password.AppendChar(c);

                    var isAuthenticated = await Auth(user, password);
                    if (isAuthenticated) return user;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error authenticating client: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    ///     Authenticates a user against the SQLite database
    /// </summary>
    /// <param name="user">The username</param>
    /// <param name="password">The password as a SecureString</param>
    /// <returns>True if authentication succeeds, false otherwise</returns>
    private static async Task<bool> Auth(string? user, SecureString password)
    {
        try
        {
            await using var connection = new SQLiteConnection("Data Source=auth.db;");
            await connection.OpenAsync();

            await using var command = new SQLiteCommand("CALL Auth(@user, @password)", connection);
            command.Parameters.AddWithValue("@user", user);
            command.Parameters.AddWithValue("@password", new NetworkCredential(string.Empty, password).Password);

            var result = (long) (await command.ExecuteScalarAsync() ?? throw new SQLiteException("SQL error"));
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
    /// <param name="certificatePath">The path of the certificate</param>
    /// <returns>The loaded X509 certificate</returns>
    public static X509Certificate2? LoadCertificate()
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
            var certificate = new X509Certificate2(certificatePath, secureString);

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
    /// Returns the UDP port from the configuration file
    /// </summary>
    /// <returns></returns>
    public static ushort GetUdpPort()
    {
        var udpPort = ConfigurationManager.AppSettings["UdpPort"] ??
                      throw new InvalidOperationException("Udp port is missing");

        return UInt16.TryParse(udpPort, out var port) ? port : DefaultUdpPort;
    }
    
    /// <summary>
    /// Returns the TCP port from the configuration file
    /// </summary>
    /// <returns></returns>
    public static ushort GetTcpPort()
    {
        var udpPort = ConfigurationManager.AppSettings["TcpPort"] ??
                      throw new InvalidOperationException("Tcp port is missing");

        return UInt16.TryParse(udpPort, out var port) ? port : DefaultTcpPort;
    }
}


/// <summary>
///     Represents a message with metadata for prioritization
/// </summary>
public class Message : IComparable<Message>
{
    public Message(string? clientId, byte[] data, int offset, int count, X509Certificate2? serverCertificate)
    {
        ClientId = clientId;

        try
        {
            using var rsa = (serverCertificate ?? throw new ArgumentNullException(nameof(serverCertificate)))
                .GetRSAPrivateKey();
            if (rsa != null) Data = rsa.Encrypt(data.AsSpan(offset, count).ToArray(), RSAEncryptionPadding.Pkcs1);
            else throw new SecurityException("RSA");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error encrypting data in Message: {ex.Message}");
            Data = Array.Empty<byte>();
        }
    }

    public string? ClientId { get; }
    public byte[] Data { get; }

    /// <summary>
    ///     Compares this message with another message based on ClientId
    /// </summary>
    /// <param name="other">The other message to compare</param>
    /// <returns>An integer indicating the relative order of the messages</returns>
    public int CompareTo(Message? other)
    {
        return other is null ? 1 : StringComparer.Ordinal.Compare(ClientId, other.ClientId);
    }
}