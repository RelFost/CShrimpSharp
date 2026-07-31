namespace CShrimpSharp;

/// <summary>
/// Describes an expected operation failure with a stable code and readable message.
/// </summary>
/// <param name="Code">A stable machine-readable error code.</param>
/// <param name="Message">A human-readable error message.</param>
/// <param name="Exception">The source exception when the failure was created at an exception boundary.</param>
public sealed record Failure(string Code, string Message, Exception? Exception = null)
{
    /// <summary>
    /// Creates a failure from an exception.
    /// </summary>
    /// <param name="exception">The source exception.</param>
    /// <returns>A failure containing the exception type and message.</returns>
    public static Failure FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new Failure(exception.GetType().Name, exception.Message, exception);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message}";
}
