namespace Lkhsoft.Dring.Messages.Authentication;

public enum AuthenticationErrorType
{
    /// <summary>
    /// Invalid username or password
    /// </summary>
    InvalidUsernameOrPassword = 0,

    /// <summary>
    /// User already exists
    /// </summary>
    UserAlreadyExists = 2,

    /// <summary>
    /// Invalid token
    /// </summary>
    InvalidToken = 3,

    /// <summary>
    /// Token expired
    /// </summary>
    TokenExpired = 4,
    
    /// <summary>
    /// Too many attempts
    /// </summary>
    TooManyAttempts = 5,
    
    /// <summary>
    /// User is blocked
    /// </summary>
    Blocked = 6,
}