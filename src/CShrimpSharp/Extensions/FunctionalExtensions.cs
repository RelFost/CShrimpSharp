namespace CShrimpSharp;

/// <summary>Provides synchronous composition helpers for <see cref="Result{TValue,TError}"/> and <see cref="Option{T}"/>.</summary>
/// <remarks>All helpers preserve the inactive branch and invoke callbacks only for the applicable branch.</remarks>
public static class FunctionalExtensions
{
    /// <summary>Transforms the present or successful value while preserving the inactive branch.</summary>
    public static Result<TOut, TError> Map<TIn, TOut, TError>(this Result<TIn, TError> result, Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess ? Result<TOut, TError>.Success(map(result.Value)) : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>Chains another optional or result-producing operation.</summary>
    public static Result<TOut, TError> Bind<TIn, TOut, TError>(this Result<TIn, TError> result, Func<TIn, Result<TOut, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        result.EnsureInitialized();
        return result.IsSuccess ? bind(result.Value) : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>Transforms a failure value while preserving a success.</summary>
    public static Result<T, TOutError> MapError<T, TError, TOutError>(this Result<T, TError> result, Func<TError, TOutError> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess ? Result<T, TOutError>.Success(result.Value) : Result<T, TOutError>.Failure(map(result.Error));
    }

    /// <summary>Preserves a success only when its value satisfies the predicate.</summary>
    public static Result<T, TError> Ensure<T, TError>(this Result<T, TError> result, Func<T, bool> predicate, Func<T, TError> error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        result.EnsureInitialized();
        return result.IsFailure || predicate(result.Value) ? result : Result<T, TError>.Failure(error(result.Value));
    }

    /// <summary>Converts a failure into a successful fallback value.</summary>
    public static Result<T, TError> Recover<T, TError>(this Result<T, TError> result, Func<TError, T> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);
        result.EnsureInitialized();
        return result.IsSuccess ? result : Result<T, TError>.Success(recover(result.Error));
    }

    /// <summary>Replaces a failure with another result.</summary>
    public static Result<T, TError> RecoverWith<T, TError>(this Result<T, TError> result, Func<TError, Result<T, TError>> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);
        result.EnsureInitialized();
        return result.IsSuccess ? result : recover(result.Error);
    }

    /// <summary>Executes a side effect for a success and returns the original result.</summary>
    public static Result<T, TError> Tap<T, TError>(this Result<T, TError> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        result.EnsureInitialized();
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    /// <summary>Executes a side effect for a failure and returns the original result.</summary>
    public static Result<T, TError> TapError<T, TError>(this Result<T, TError> result, Action<TError> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        result.EnsureInitialized();
        if (result.IsFailure) action(result.Error);
        return result;
    }

    /// <summary>Returns the contained value or an eager fallback.</summary>
    public static T GetValueOr<T, TError>(this Result<T, TError> result, T fallback)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? result.Value : fallback;
    }

    /// <summary>Returns the contained value or computes a fallback lazily.</summary>
    public static T GetValueOrElse<T, TError>(this Result<T, TError> result, Func<TError, T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        result.EnsureInitialized();
        return result.IsSuccess ? result.Value : fallback(result.Error);
    }

    /// <summary>Converts a successful result to Some and a failure to None.</summary>
    public static Option<T> ToOption<T, TError>(this Result<T, TError> result)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? Option.Some(result.Value) : Option.None<T>();
    }

    /// <summary>Transforms the present or successful value while preserving the inactive branch.</summary>
    public static Option<TOut> Map<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return option.IsSome ? Option.Some(map(option.Value)) : Option.None<TOut>();
    }

    /// <summary>Chains another optional or result-producing operation.</summary>
    public static Option<TOut> Bind<TIn, TOut>(this Option<TIn> option, Func<TIn, Option<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return option.IsSome ? bind(option.Value) : Option.None<TOut>();
    }

    /// <summary>Keeps a present option only when its value satisfies the predicate.</summary>
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return option.IsSome && predicate(option.Value) ? option : Option.None<T>();
    }

    /// <summary>Returns the contained value or an eager fallback.</summary>
    public static T GetValueOr<T>(this Option<T> option, T fallback) => option.IsSome ? option.Value : fallback;

    /// <summary>Returns the contained value or computes a fallback lazily.</summary>
    public static T GetValueOrElse<T>(this Option<T> option, Func<T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.IsSome ? option.Value : fallback();
    }

    /// <summary>Returns this option when present, otherwise computes another option.</summary>
    public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.IsSome ? option : fallback();
    }

    /// <summary>Converts Some to success and None to a lazily created failure.</summary>
    public static Result<T, TError> ToResult<T, TError>(this Option<T> option, Func<TError> error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return option.IsSome ? Result<T, TError>.Success(option.Value) : Result<T, TError>.Failure(error());
    }
}
