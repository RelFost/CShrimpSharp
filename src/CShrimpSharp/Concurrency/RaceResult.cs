namespace CShrimpSharp.Concurrency;
public readonly record struct RaceResult<T>(int WinnerIndex, T Value);
