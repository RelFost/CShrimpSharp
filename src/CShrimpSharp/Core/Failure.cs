namespace CShrimpSharp;

public sealed record Failure(string Code, string Message, Exception? Exception = null)
{
    public static Failure FromException(Exception exception) =>
        new(exception.GetType().Name, exception.Message, exception);

    public override string ToString() => $"{Code}: {Message}";
}
