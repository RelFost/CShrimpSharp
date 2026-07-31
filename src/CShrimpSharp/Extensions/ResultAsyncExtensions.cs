namespace CShrimpSharp;

/// <summary>
/// Asynchronous composition helpers for result values.
/// </summary>
public static class ResultAsyncExtensions
{
    /// <summary>
    /// Asynchronously maps a successful value while preserving failures.
    /// </summary>
    public static async ValueTask<Result<TOutput, TError>> MapAsync<TValue, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, CancellationToken, ValueTask<TOutput>> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (result.IsFailure)
        {
            return Result<TOutput, TError>.Failure(result.Error);
        }

        TValue value = result.Value;
        TOutput output = await map(value, cancellationToken).ConfigureAwait(false);
        return Result<TOutput, TError>.Success(output);
    }

    /// <summary>
    /// Asynchronously chains a result-producing operation after a success.
    /// </summary>
    public static async ValueTask<Result<TOutput, TError>> BindAsync<TValue, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, CancellationToken, ValueTask<Result<TOutput, TError>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bind);

        if (result.IsFailure)
        {
            return Result<TOutput, TError>.Failure(result.Error);
        }

        return await bind(result.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously executes a side effect for a success and returns the original result.
    /// </summary>
    public static async ValueTask<Result<TValue, TError>> TapAsync<TValue, TError>(
        this Result<TValue, TError> result,
        Func<TValue, CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            await action(result.Value, cancellationToken).ConfigureAwait(false);
            return result;
        }

        result.EnsureInitialized();
        return result;
    }

    /// <summary>
    /// Asynchronously maps a result produced by a <see cref="ValueTask{TResult}" />.
    /// </summary>
    public static async ValueTask<Result<TOutput, TError>> MapAsync<TValue, TOutput, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, CancellationToken, ValueTask<TOutput>> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        Result<TValue, TError> result = await source.ConfigureAwait(false);
        return await result.MapAsync(map, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously chains a result produced by a <see cref="ValueTask{TResult}" />.
    /// </summary>
    public static async ValueTask<Result<TOutput, TError>> BindAsync<TValue, TOutput, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, CancellationToken, ValueTask<Result<TOutput, TError>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bind);

        Result<TValue, TError> result = await source.ConfigureAwait(false);
        return await result.BindAsync(bind, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously maps a result produced by a <see cref="Task{TResult}" />.
    /// </summary>
    public static ValueTask<Result<TOutput, TError>> MapAsync<TValue, TOutput, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, CancellationToken, ValueTask<TOutput>> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(map);

        return new ValueTask<Result<TValue, TError>>(source)
            .MapAsync(map, cancellationToken);
    }

    /// <summary>
    /// Asynchronously chains a result produced by a <see cref="Task{TResult}" />.
    /// </summary>
    public static ValueTask<Result<TOutput, TError>> BindAsync<TValue, TOutput, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, CancellationToken, ValueTask<Result<TOutput, TError>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(bind);

        return new ValueTask<Result<TValue, TError>>(source)
            .BindAsync(bind, cancellationToken);
    }
}
