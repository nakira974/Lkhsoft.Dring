#region

using System.Text.Json.Serialization;

#endregion

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// User information
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