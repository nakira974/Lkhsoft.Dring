#region

using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Lkhsoft.Dring.Client.Utilities;

/// <summary>
/// String extensions for the application
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a SecureString to a clear text string
    /// </summary>
    /// <param name="secureString">SecureString to be converted to clear text</param>
    /// <returns>A clear text of the given SecureString or null if the unmanaged pointer is null</returns>
    public static string? ToClearText(this SecureString secureString)
    {
        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(ptr);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    /// <summary>
    /// Get a deterministic IV from the username to use in encryption
    /// </summary>
    /// <param name="username">Username to be used to generate the IV</param>
    /// <returns>IV for the current username</returns>
    public static string GetDeterministicIv(string username)
    {
        // Convertir l'username en tableau de bytes
        var usernameBytes = Encoding.UTF8.GetBytes(username);

        // Utiliser SHA256 pour générer un hash de l'username
        var hash = SHA256.HashData(usernameBytes);

        // Prendre les 8 premiers octets du hash
        var iv = new byte[8];
        Array.Copy(hash, iv, 8);

        // Convertir les 8 octets en une chaîne hexadécimale
        return BitConverter.ToString(iv).Replace("-", string.Empty);
    }
}