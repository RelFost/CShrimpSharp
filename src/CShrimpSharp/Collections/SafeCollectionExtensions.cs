namespace CShrimpSharp.Collections;

public static class SafeCollectionExtensions
{
    public static Option<T> AtOrNone<T>(this IReadOnlyList<T> source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (uint)index < (uint)source.Count ? Option.Some(source[index]) : Option.None<T>();
    }
    public static Option<TValue> FindOrNone<TKey,TValue>(this IReadOnlyDictionary<TKey,TValue> source, TKey key) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.TryGetValue(key, out TValue? value) ? Option.Some(value) : Option.None<TValue>();
    }
    public static Result<T,Failure> SingleResult<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using IEnumerator<T> e=source.GetEnumerator();
        if(!e.MoveNext()) return Result.Failure<T>(new Failure("sequence_empty","The sequence is empty."));
        T value=e.Current;
        return e.MoveNext() ? Result.Failure<T>(new Failure("sequence_multiple","The sequence contains more than one element.")) : Result.Success(value);
    }
}
