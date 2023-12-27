namespace Suspense.Server.Exceptions;

/// <summary>
/// Exception that represents a general error related to game operations.
/// </summary>
public class GameException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class with a default error message.
    /// </summary>
    public GameException() : base("An error occurred during game operation.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    public GameException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class with a custom error message and an inner exception.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception that caused this exception to be thrown.</param>
    public GameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}