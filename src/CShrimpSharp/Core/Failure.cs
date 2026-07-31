namespace CShrimpSharp;

/// <summary>
/// Describes a failure as data instead of using an exception for ordinary control flow.
/// </summary>
public sealed record Failure
{
    /// <summary>
    /// Initializes a new failure.
    /// </summary>
    /// <param name="code">A stable machine-readable code.</param>
    /// <param name="message">A human-readable description.</param>
    /// <param name="exception">The optional exception that caused the failure.</param>
    public Failure(string code, string message, Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// Gets the stable machine-readable failure code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable failure message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional exception that caused the failure.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Creates a failure from an exception.
    /// </summary>
    /// <param name="exception">The source exception.</param>
    /// <param name="code">The stable failure code.</param>
    /// <returns>A failure containing the exception.</returns>
    public static Failure FromException(Exception exception, string code = "unexpected_error")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        string message = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;

        return new Failure(code, message, exception);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message}";
}
