#region

using System.Text.Json.Serialization;

#endregion

namespace Lkhsoft.Dring.Client.Models;

/// <summary>
/// User information
/// </summary>
public record AppUser
{
    /// <summary>
    /// Username of the connected user
    /// </summary>
    [JsonPropertyName("username")]
    public string UserName { get; set; }

    /// <summary>
    /// Encrypted password of the connected user
    /// </summary>
    [JsonPropertyName("encryptedPassword")]
    public string EncryptedPassword { get; set; }
}