namespace CShrimpSharp.Concurrency;

/// <summary>
/// Contains the winning value and zero-based operation index from a race.
/// </summary>
/// <typeparam name="T">The winning value type.</typeparam>
/// <param name="WinnerIndex">The zero-based index of the winning operation.</param>
/// <param name="Value">The winning value.</param>
public readonly record struct RaceResult<T>(int WinnerIndex, T Value);
