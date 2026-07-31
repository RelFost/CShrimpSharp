namespace CShrimpSharp;

/// <summary>
/// The exception thrown when the value or error of a <see cref="Result{TValue,TError}" />
/// is read from the wrong state.
/// </summary>
public sealed class InvalidResultAccessException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidResultAccessException(string message)
        : base(message)
    {
    }
}
