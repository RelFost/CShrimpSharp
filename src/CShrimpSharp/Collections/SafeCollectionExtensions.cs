namespace CShrimpSharp.Collections;

/// <summary>
/// Provides safe collection operations that return <see cref="Option{T}" />
/// or <see cref="Result{TValue,TError}" /> instead of throwing for expected absence.
/// </summary>
public static class SafeCollectionExtensions
{
    /// <summary>
    /// Returns the element at <paramref name="index" />, or <see cref="Option{T}.None" />
    /// when the index is outside the collection.
    /// </summary>
    public static Option<T> AtOrNone<T>(this IReadOnlyList<T> source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);

        return (uint)index < (uint)source.Count
            ? Option.Some(source[index])
            : Option.None<T>();
    }

    /// <summary>
    /// Returns the value for <paramref name="key" />, or an empty option when no entry exists.
    /// </summary>
    public static Option<TValue> FindOrNone<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source,
        TKey key)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.TryGetValue(key, out TValue? value)
            ? Option.Some(value)
            : Option.None<TValue>();
    }

    /// <summary>
    /// Returns the single sequence element, or a typed failure when the sequence
    /// is empty or contains more than one element.
    /// </summary>
    public static Result<T, Failure> SingleResult<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return Result.Failure<T>(
                new Failure("sequence_empty", "The sequence is empty."));
        }

        T value = enumerator.Current;

        return enumerator.MoveNext()
            ? Result.Failure<T>(
                new Failure(
                    "sequence_multiple",
                    "The sequence contains more than one element."))
            : Result.Success(value);
    }
}
