namespace Suspense.Server.Exceptions;

/// <summary>
/// Exception that represents a general error related to player operations.
/// </summary>
public class PlayerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerException"/> class with a default error message.
    /// </summary>
    public PlayerException() : base("An error occurred during player operation.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    public PlayerException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerException"/> class with a custom error message and an inner exception.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception that caused this exception to be thrown.</param>
    public PlayerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}