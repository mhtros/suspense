namespace Suspense.Server.Exceptions;

/// <summary>
/// Exception that represents a general error related to entity operations.
/// </summary>
public class EntityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class with a default error message.
    /// </summary>
    public EntityException() : base("An error occurred during entity operation.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    public EntityException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityException"/> class with a custom error message and an inner exception.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception that caused this exception to be thrown.</param>
    public EntityException(string message, Exception innerException) : base(message, innerException)
    {
    }
}