namespace CShrimpSharp.Concurrency;

/// <summary>
/// Configures the behavior of a <see cref="ShrimpScope" />.
/// </summary>
public sealed record ShrimpScopeOptions
{
    /// <summary>
    /// Gets the default scope options.
    /// </summary>
    public static ShrimpScopeOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether a failed branch cancels sibling branches.
    /// </summary>
    public bool CancelSiblingsOnFailure { get; init; } = true;
}
