#region

using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Lkhsoft.Dring.Messages;
using Lkhsoft.Dring.Messages.Authentication;

#endregion

namespace Lkhsoft.Dring.Client.Services.Authentication;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    /// <summary>
    /// Tcp client for the connection
    /// </summary>
    private TcpClient _tcpClient { get; set; } = new();

    /// <inheritdoc/>
    public SslStream? SslStream { get; set; }

    /// <inheritdoc/>
    public async Task<bool> LogOn(string username, SecureString password)
    {
        var iv = StringExtensions.GetDeterministicIv(username);
        var encryptedPassword = EncryptWithVersion(password, Encoding.UTF8.GetBytes(iv));
        bool result;

        var jsonSerializationOptions = new JsonSerializerOptions() {WriteIndented = false, DefaultBufferSize = 2048};

        var appUser = new ConnectedUser {UserName = username, EncryptedPassword = encryptedPassword ?? string.Empty};
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync<ConnectedUser>(stream, appUser, jsonSerializationOptions);

        var authMessage = new RequestMessage(MessageType.Authentication, RequestMessageType.Login, stream);
        stream = new MemoryStream();
        await JsonSerializer.SerializeAsync<RequestMessage>(stream, authMessage, jsonSerializationOptions);

        try
        {
            if (!_tcpClient.Connected)
            {
                await _tcpClient.ConnectAsync("localhost", 9091);
                SslStream = new SslStream(_tcpClient.GetStream(), true,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate));
                SslStream.ReadTimeout = 30000;
                await SslStream.AuthenticateAsClientAsync("localhost");
            }

            if (SslStream is not null)
            {
                await SslStream.WriteAsync(stream.ToArray());
                var authData = new byte[1024];
                var byteRead = await SslStream.ReadAsync(authData, 0, authData.Length);
                Array.Resize(ref authData, byteRead);
                var response = JsonSerializer.Deserialize<ResponseMessage>(authData);
                if (response is null)
                    throw new AuthenticationException("Authentication failed: invalid server response");

                if (response is {Header: not null} &&
                    (response.Header.Type == ResponseMessageType.None ||
                     response.Header.Type == ResponseMessageType.Warning ||
                     response.Body.Data is null ||
                     response.Header.MessageLength == 0))

                    throw new AuthenticationException("Authentication failed: invalid response");
                result = false;
                if (response.Body.Data is not null && response.Header is not null)
                    if (response.Header is {Type : ResponseMessageType.Error})
                    {
                        var authenticationError =
                            JsonSerializer.Deserialize<AuthenticationErrorMessage>(response.Body.Data);
                        if (authenticationError is null)
                            throw new AuthenticationException("Authentication failed: invalid response");
                        
                        if (authenticationError.ErrorType == AuthenticationErrorType.TooManyAttempts)
                        {
                            _tcpClient.Close();
                            _tcpClient = new TcpClient();
                        }
                        throw new AuthenticationException(authenticationError.Message);
                    }

                if (response.Header is {Type : ResponseMessageType.Success})
                    return true;
            }
            else
            {
                throw new AuthenticationException("Authentication failed: SSL stream is null");
            }
        }
        catch (Exception e)
        {
            throw new AuthenticationException($"Authentication failed: {e.Message}");
        }

        return result;
    }

    public Task<bool> LogOff()
    {
        throw new NotImplementedException();
    }

    #region PRIVATE METHODS

    /// <summary>
    /// SSL server certificate validation
    /// </summary>
    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // En mode DEBUG, accepter les certificats auto-signés
#if DEBUG
        if (sslPolicyErrors == SslPolicyErrors.None ||
            sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            return true;
#else
    // En mode RELEASE, appliquer une validation stricte
    if (sslPolicyErrors == SslPolicyErrors.None)
    {
        return true;
    }
#endif

        Console.WriteLine("Erreur de certificat SSL : " + sslPolicyErrors);
        return false;
    }

    /// <summary>
    /// Map of AES keys by version number
    /// </summary>
    private static readonly Dictionary<int, byte[]> KeyVersions = new()
    {
        {1, Convert.FromBase64String("9WpFqL8J7g5dYq8B5jK9nl6jPjdMN1FobNfhz0axdkM=")},
        {2, Convert.FromBase64String("tDF3v6G3PqNbIh7D8HSTGpN9oYfrXfH76nboydHpCeY=")}
    };

    /// <summary>
    /// Encrypt the secure string with the given version
    /// </summary>
    private static string? EncryptWithVersion(SecureString? secureString, byte[] iv)

    {
        if (!KeyVersions.TryGetValue(2, out var key))
            throw new ArgumentException("Invalid key version");

        byte[] numArray;
        var num = IntPtr.Zero;
        try
        {
            num = Marshal.SecureStringToGlobalAllocUnicode(secureString ??
                                                           throw new ArgumentNullException(nameof(secureString)));
            var stringUni = Marshal.PtrToStringUni(num);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var memoryStream = new MemoryStream();
            memoryStream.WriteByte(2);
            memoryStream.Write(aes.IV, 0, aes.IV.Length);
            using (var cryptoStream =
                   new CryptoStream((Stream) memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                using (var streamWriter = new StreamWriter((Stream) cryptoStream, Encoding.UTF8))
                {
                    streamWriter.Write(stringUni);
                }
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        finally
        {
            if (num != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(num);
        }
    }

    #endregion
}