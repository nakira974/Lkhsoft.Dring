using System.Text.Json.Serialization;

namespace Lkhsoft.Dring.Messages.Authentication;

/// <summary>
/// Error message for authentication
/// </summary>
public class AuthenticationErrorMessage : ErrorMessage<AuthenticationErrorType>
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public AuthenticationErrorMessage() : base()
    {
        
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="errorType">Authentication error type</param>
    /// <param name="message">Authentication error message</param>
    public AuthenticationErrorMessage(AuthenticationErrorType errorType, string message) : base(errorType, message)
    {
        
    }
}