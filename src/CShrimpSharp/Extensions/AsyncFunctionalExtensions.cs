namespace CShrimpSharp;

/// <summary>Provides asynchronous composition helpers for <see cref="Result{TValue,TError}"/> and <see cref="Option{T}"/>.</summary>
/// <remarks>Callbacks receive the caller-provided cancellation token and are not invoked for inactive branches.</remarks>
public static class AsyncFunctionalExtensions
{
    /// <summary>Asynchronously transforms a present or successful value while preserving the inactive branch.</summary>
    public static async ValueTask<Result<TOut, TError>> MapAsync<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, CancellationToken, ValueTask<TOut>> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess
            ? Result<TOut, TError>.Success(await map(result.Value, cancellationToken).ConfigureAwait(false))
            : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>Asynchronously chains another optional or result-producing operation.</summary>
    public static async ValueTask<Result<TOut, TError>> BindAsync<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, CancellationToken, ValueTask<Result<TOut, TError>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bind);
        result.EnsureInitialized();
        return result.IsSuccess
            ? await bind(result.Value, cancellationToken).ConfigureAwait(false)
            : Result<TOut, TError>.Failure(result.Error);
    }

    /// <summary>Asynchronously projects the active result branch into a single value.</summary>
    public static async ValueTask<TResult> MatchAsync<T, TError, TResult>(
        this Result<T, TError> result,
        Func<T, CancellationToken, ValueTask<TResult>> success,
        Func<TError, CancellationToken, ValueTask<TResult>> failure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        result.EnsureInitialized();
        return result.IsSuccess
            ? await success(result.Value, cancellationToken).ConfigureAwait(false)
            : await failure(result.Error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Asynchronously transforms a present or successful value while preserving the inactive branch.</summary>
    public static async ValueTask<Option<TOut>> MapAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, CancellationToken, ValueTask<TOut>> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        return option.IsSome
            ? Option.Some(await map(option.Value, cancellationToken).ConfigureAwait(false))
            : Option.None<TOut>();
    }

    /// <summary>Asynchronously chains another optional or result-producing operation.</summary>
    public static async ValueTask<Option<TOut>> BindAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, CancellationToken, ValueTask<Option<TOut>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return option.IsSome
            ? await bind(option.Value, cancellationToken).ConfigureAwait(false)
            : Option.None<TOut>();
    }
}
