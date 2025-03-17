#region

using System.Text.Json.Serialization;

#endregion

namespace Lkhsoft.Dring.Server.Utility.Core;

/// <summary>
/// Connected user information for the server
/// </summary>
public record ConnectedUser
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