using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages;

/// <summary>
/// RPC error message
/// </summary>
public class ErrorMessage<TError> where TError : Enum
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public ErrorMessage()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="errorType">Error type</param>
    /// <param name="message">Error message</param>
    public ErrorMessage(TError errorType , string message)
    {
        ErrorType = errorType;
        Message = message;
    }
    
    /// <summary>
    /// Error type
    /// </summary>
    [JsonPropertyName("error_type")]
    public TError? ErrorType { get; init; }
    
    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? Message { get; init; }
}