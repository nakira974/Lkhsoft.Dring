using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Lkhsoft.Dring.Server.Utility;

namespace Lkhsoft.Dring.Server
{
    class Program
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static Semaphore _cliSemaphore = new Semaphore(1, 1);
        private static readonly ConcurrentDictionary<string, NetworkStream> Clients = new ConcurrentDictionary<string, NetworkStream>();
        private static readonly ConcurrentDictionary<string, UdpClient?> SendChannels = new ConcurrentDictionary<string, UdpClient?>();
        private static readonly PriorityQueue<Message, byte> GlobalPriorityQueue = new PriorityQueue<Message, byte>();
        private static readonly X509Certificate2? ServerCertificate  = LoadCertificate("server_certificate.pfx");

        static Task Main(string[] args)
        {
            Console.WriteLine("Server is starting...");
            NLogConfigurator.ConfigureFromYaml("server.yaml");
            
            _ = Task.Run(() => ReceiveTcp());
            _ = Task.Run(() => ReceiveUdp());
            _ = Task.Run(() => Transmit());

            while (true)
            {
                Console.WriteLine("CLI Ready. Type 'exit' to quit.");
                var input = Console.ReadLine();
                if (input?.Trim().ToLower() == "exit")
                {
                    break;
                }
            }

            Console.WriteLine("Shutting down...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Receives messages from TCP clients
        /// </summary>
        private static async Task ReceiveTcp()
        {
            var listener = new TcpListener(IPAddress.Any, 12345);
            listener.Start();
            Console.WriteLine("TCP listener started on port 12345.");

            while (true)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var stream = client.GetStream();

                    // Authenticate client
                    var id = await AuthenticateClient(stream);
                    if (String.IsNullOrEmpty(id))
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
        }

        /// <summary>
        /// Receives messages from UDP clients
        /// </summary>
        private static void ReceiveUdp()
        {
            var udpListener = new UdpClient(54321);
            Console.WriteLine("UDP listener started on port 54321.");

            while (true)
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
        /// Processes messages for a specific client
        /// </summary>
        /// <param name="idClient">The ID of the client</param>
        /// <param name="stream">The client's stream</param>
        private static async Task Job(string? idClient, NetworkStream stream)
        {
            while (stream.CanRead)
            {
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
        }

        /// <summary>
        /// Transmits messages via UDP based on the global priority queue
        /// </summary>
        private static async Task Transmit()
        {
            while (true)
            {
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

                    if (SendChannels.TryGetValue(message.ClientId ?? throw new InvalidOperationException("Client id is null"), out UdpClient? udpClient))
                    {
                        if (udpClient != null) await udpClient.SendAsync(message.Data, message.Data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in Transmit loop: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Authenticates a client using a certificate and verifies user credentials
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
                    using var rsa = (ServerCertificate ?? throw new InvalidOperationException("Invalid certificate")).GetRSAPrivateKey();
                    var decryptedData = rsa?.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
                    string?[] authData = Encoding.UTF8.GetString(decryptedData ?? Array.Empty<byte>()).Split(':');

                    if (authData.Length == 2)
                    {
                        var user = authData[0];
                        var password = new SecureString();
                        foreach (var c in authData[1]!)
                        {
                            password.AppendChar(c);
                        }

                        var isAuthenticated = await Auth(user, password);
                        if (isAuthenticated)
                        {
                            return user;
                        }
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
        /// Authenticates a user against the SQLite database
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
                command.Parameters.AddWithValue("@password", new System.Net.NetworkCredential(string.Empty, password).Password);

                var result = (long)(await command.ExecuteScalarAsync() ?? throw new SQLiteException("SQL error"));
                return result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Auth: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the server certificate from the local store
        /// </summary>
        /// <param name="certificatePath">The path of the certificate</param>
        /// <returns>The loaded X509 certificate</returns>
        public static X509Certificate2? LoadCertificate(string certificatePath)
        {
            try
            {
                // Load the certificate from the .pfx file
                // Retrieve the plain-text password from the config file
                var passwordPlain = ConfigurationManager.AppSettings["CertificatePassword"] ?? throw new InvalidOperationException("Certificate password not found");
                var secureString = new SecureString();
                foreach (var c in passwordPlain)
                {
                    secureString.AppendChar(c);
                }
                secureString.MakeReadOnly();  // Make the SecureString immutable

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
    }

    /// <summary>
    /// Represents a message with metadata for prioritization
    /// </summary>
    public class Message : IComparable<Message>
    {
        public string? ClientId { get; }
        public byte[] Data { get; }

        public Message(string? clientId, byte[] data, int offset, int count, X509Certificate2? serverCertificate)
        {
            ClientId = clientId;

            try
            {
                using var rsa = (serverCertificate ?? throw new ArgumentNullException(nameof(serverCertificate))).GetRSAPrivateKey();
                if (rsa != null) Data = rsa.Encrypt(data.AsSpan(offset, count).ToArray(), RSAEncryptionPadding.Pkcs1);
                else throw new SecurityException("RSA");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encrypting data in Message: {ex.Message}");
                Data = Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Compares this message with another message based on ClientId
        /// </summary>
        /// <param name="other">The other message to compare</param>
        /// <returns>An integer indicating the relative order of the messages</returns>
        public int CompareTo(Message? other)
        {
            return other is null ? 1 : StringComparer.Ordinal.Compare(ClientId, other.ClientId);
        }
    }
}
