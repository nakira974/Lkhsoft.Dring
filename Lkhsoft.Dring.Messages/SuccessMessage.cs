using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC success message
/// </summary>
public class SuccessMessage
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public SuccessMessage()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="message">Success message</param>
    public SuccessMessage(string? message)
    {
        Message = message;
    }
    
    /// <summary>
    /// Success message
    /// </summary>
    [JsonPropertyName("success_message")]
    public string? Message { get; init; }
}