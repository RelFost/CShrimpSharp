namespace CShrimpSharp.Concurrency;

/// <summary>
/// Contains the result of a race and the zero-based index of the winning operation.
/// </summary>
/// <typeparam name="TValue">The operation result type.</typeparam>
public readonly record struct RaceResult<TValue>
{
    /// <summary>
    /// Initializes a race result.
    /// </summary>
    /// <param name="winnerIndex">The zero-based index of the winning operation.</param>
    /// <param name="value">The value returned by the winner.</param>
    public RaceResult(int winnerIndex, TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(winnerIndex);
        WinnerIndex = winnerIndex;
        Value = value;
    }

    /// <summary>
    /// Gets the zero-based index of the winning operation.
    /// </summary>
    public int WinnerIndex { get; }

    /// <summary>
    /// Gets the value returned by the winning operation.
    /// </summary>
    public TValue Value { get; }
}
