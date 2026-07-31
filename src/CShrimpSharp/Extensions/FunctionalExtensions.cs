namespace CShrimpSharp;

/// <summary>Provides functional composition helpers for result and option values.</summary>
public static class FunctionalExtensions
{
    public static Result<TOut, TError> Map<TIn, TOut, TError>(this Result<TIn, TError> result, Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess ? Result<TOut, TError>.Success(map(result.Value)) : Result<TOut, TError>.Failure(result.Error);
    }

    public static Result<TOut, TError> Bind<TIn, TOut, TError>(this Result<TIn, TError> result, Func<TIn, Result<TOut, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        result.EnsureInitialized();
        return result.IsSuccess ? bind(result.Value) : Result<TOut, TError>.Failure(result.Error);
    }

    public static Result<T, TError> Ensure<T, TError>(this Result<T, TError> result, Func<T, bool> predicate, Func<T, TError> error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        result.EnsureInitialized();
        return result.IsFailure || predicate(result.Value) ? result : Result<T, TError>.Failure(error(result.Value));
    }

    public static Result<TValue, TOutError> MapError<TValue, TError, TOutError>(this Result<TValue, TError> result, Func<TError, TOutError> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess ? Result<TValue, TOutError>.Success(result.Value) : Result<TValue, TOutError>.Failure(map(result.Error));
    }

    public static Result<TValue, TError> Recover<TValue, TError>(this Result<TValue, TError> result, Func<TError, TValue> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);
        result.EnsureInitialized();
        return result.IsSuccess ? result : Result<TValue, TError>.Success(recover(result.Error));
    }

    public static Result<TValue, TError> RecoverWith<TValue, TError>(this Result<TValue, TError> result, Func<TError, Result<TValue, TError>> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);
        result.EnsureInitialized();
        return result.IsSuccess ? result : recover(result.Error);
    }

    public static Result<TValue, TError> Tap<TValue, TError>(this Result<TValue, TError> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        result.EnsureInitialized();
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    public static Result<TValue, TError> TapError<TValue, TError>(this Result<TValue, TError> result, Action<TError> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        result.EnsureInitialized();
        if (result.IsFailure) action(result.Error);
        return result;
    }

    public static TValue GetValueOr<TValue, TError>(this Result<TValue, TError> result, TValue fallback)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? result.Value : fallback;
    }

    public static TValue GetValueOrElse<TValue, TError>(this Result<TValue, TError> result, Func<TError, TValue> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        result.EnsureInitialized();
        return result.IsSuccess ? result.Value : fallback(result.Error);
    }

    public static Option<TValue> ToOption<TValue, TError>(this Result<TValue, TError> result)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? Option.Some(result.Value) : Option.None<TValue>();
    }

    public static Option<TOut> Map<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return option.IsSome ? Option.Some(map(option.Value)) : Option.None<TOut>();
    }

    public static Option<TOut> Bind<TIn, TOut>(this Option<TIn> option, Func<TIn, Option<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return option.IsSome ? bind(option.Value) : Option.None<TOut>();
    }

    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return option.IsSome && predicate(option.Value) ? option : Option.None<T>();
    }

    public static Option<T> Tap<T>(this Option<T> option, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (option.IsSome) action(option.Value);
        return option;
    }

    public static T GetValueOr<T>(this Option<T> option, T fallback) => option.IsSome ? option.Value : fallback;

    public static T GetValueOrElse<T>(this Option<T> option, Func<T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.IsSome ? option.Value : fallback();
    }

    public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.IsSome ? option : fallback();
    }

    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return option.IsSome ? Result<T, TError>.Success(option.Value) : Result<T, TError>.Failure(error());
    }
}
