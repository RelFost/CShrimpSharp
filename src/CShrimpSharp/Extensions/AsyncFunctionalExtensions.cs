namespace CShrimpSharp;

/// <summary>Provides asynchronous composition helpers for result and option values.</summary>
public static class AsyncFunctionalExtensions
{
    public static async ValueTask<Result<TOut, TError>> MapAsync<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, ValueTask<TOut>> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        result.EnsureInitialized();
        return result.IsSuccess
            ? Result<TOut, TError>.Success(await map(result.Value).ConfigureAwait(false))
            : Result<TOut, TError>.Failure(result.Error);
    }

    public static async ValueTask<Result<TOut, TError>> BindAsync<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, ValueTask<Result<TOut, TError>>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        result.EnsureInitialized();
        return result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<TOut, TError>.Failure(result.Error);
    }

    public static async ValueTask<TResult> MatchAsync<TValue, TError, TResult>(
        this Result<TValue, TError> result,
        Func<TValue, ValueTask<TResult>> success,
        Func<TError, ValueTask<TResult>> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        result.EnsureInitialized();
        return result.IsSuccess
            ? await success(result.Value).ConfigureAwait(false)
            : await failure(result.Error).ConfigureAwait(false);
    }

    public static async ValueTask<Option<TOut>> MapAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, ValueTask<TOut>> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return option.IsSome
            ? Option.Some(await map(option.Value).ConfigureAwait(false))
            : Option.None<TOut>();
    }

    public static async ValueTask<Option<TOut>> BindAsync<TIn, TOut>(
        this Option<TIn> option,
        Func<TIn, ValueTask<Option<TOut>>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return option.IsSome
            ? await bind(option.Value).ConfigureAwait(false)
            : Option.None<TOut>();
    }
}
