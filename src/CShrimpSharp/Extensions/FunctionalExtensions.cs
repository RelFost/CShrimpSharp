namespace CShrimpSharp;

/// <summary>
/// Provides functional composition helpers for <see cref="Result{TValue,TError}" />
/// and <see cref="Option{T}" /> values.
/// </summary>
public static class FunctionalExtensions
{
    /// <summary>
    /// Transforms a successful result while preserving a failure.
    /// </summary>
    public static Result<TOut, TError> Map<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();

        return result.IsSuccess
            ? Result<TOut, TError>.Success(map(result.Value))
            : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>
    /// Sequences another result-producing operation after a success.
    /// </summary>
    public static Result<TOut, TError> Bind<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, Result<TOut, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        result.EnsureInitialized();

        return result.IsSuccess
            ? bind(result.Value)
            : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>
    /// Converts a successful result into a failure when a predicate is not satisfied.
    /// </summary>
    public static Result<T, TError> Ensure<T, TError>(
        this Result<T, TError> result,
        Func<T, bool> predicate,
        Func<T, TError> error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        result.EnsureInitialized();

        if (result.IsFailure || predicate(result.Value))
        {
            return result;
        }

        return Result<T, TError>.Failure(error(result.Value));
    }

    /// <summary>
    /// Transforms a present optional value while preserving an empty option.
    /// </summary>
    public static Option<TOut> Map<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return option.IsSome
            ? Option.Some(map(option.Value))
            : Option.None<TOut>();
    }

    /// <summary>
    /// Preserves a present optional value only when it satisfies a predicate.
    /// </summary>
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return option.IsSome && predicate(option.Value) ? option : Option.None<T>();
    }

    /// <summary>
    /// Returns the optional value or a fallback when the option is empty.
    /// </summary>
    public static T GetValueOr<T>(this Option<T> option, T fallback) =>
        option.IsSome ? option.Value : fallback;
}
