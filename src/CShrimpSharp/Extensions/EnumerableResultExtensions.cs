namespace CShrimpSharp;

/// <summary>
/// Collection helpers for composing result values.
/// </summary>
public static class EnumerableResultExtensions
{
    /// <summary>
    /// Converts a sequence of results into one result containing all values.
    /// The first failure stops enumeration.
    /// </summary>
    public static Result<IReadOnlyList<TValue>, TError> Sequence<TValue, TError>(
        this IEnumerable<Result<TValue, TError>> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var values = new List<TValue>();

        foreach (Result<TValue, TError> result in source)
        {
            if (result.IsFailure)
            {
                return Result<IReadOnlyList<TValue>, TError>.Failure(result.Error);
            }

            values.Add(result.Value);
        }

        return Result<IReadOnlyList<TValue>, TError>.Success(values.ToArray());
    }

    /// <summary>
    /// Maps each source item to a result and collects all successful values.
    /// The first failure stops enumeration.
    /// </summary>
    public static Result<IReadOnlyList<TValue>, TError> Traverse<TSource, TValue, TError>(
        this IEnumerable<TSource> source,
        Func<TSource, Result<TValue, TError>> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var values = new List<TValue>();

        foreach (TSource item in source)
        {
            Result<TValue, TError> result = selector(item);

            if (result.IsFailure)
            {
                return Result<IReadOnlyList<TValue>, TError>.Failure(result.Error);
            }

            values.Add(result.Value);
        }

        return Result<IReadOnlyList<TValue>, TError>.Success(values.ToArray());
    }
}
