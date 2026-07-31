namespace CShrimpSharp;

/// <summary>
/// Represents a successful operation that does not return a meaningful value.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// Gets the single logical unit value.
    /// </summary>
    public static Unit Value => default;

    /// <inheritdoc />
    public override string ToString() => "()";
}
