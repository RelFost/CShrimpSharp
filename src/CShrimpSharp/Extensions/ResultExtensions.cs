namespace CShrimpSharp;

/// <summary>
/// Functional composition helpers for <see cref="Result{TValue,TError}" />.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful value while preserving failures.
    /// </summary>
    public static Result<TOutput, TError> Map<TValue, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, TOutput> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return result.Match(
            value => Result<TOutput, TError>.Success(map(value)),
            Result<TOutput, TError>.Failure);
    }

    /// <summary>
    /// Maps a failure value while preserving successes.
    /// </summary>
    public static Result<TValue, TOutputError> MapError<TValue, TError, TOutputError>(
        this Result<TValue, TError> result,
        Func<TError, TOutputError> mapError)
    {
        ArgumentNullException.ThrowIfNull(mapError);

        return result.Match(
            Result<TValue, TOutputError>.Success,
            error => Result<TValue, TOutputError>.Failure(mapError(error)));
    }

    /// <summary>
    /// Chains a result-producing operation after a successful result.
    /// </summary>
    public static Result<TOutput, TError> Bind<TValue, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, Result<TOutput, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        return result.Match(bind, Result<TOutput, TError>.Failure);
    }

    /// <summary>
    /// Converts a success into a failure when the predicate is false.
    /// </summary>
    public static Result<TValue, TError> Ensure<TValue, TError>(
        this Result<TValue, TError> result,
        Func<TValue, bool> predicate,
        Func<TValue, TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        return result.Bind(value => predicate(value)
            ? Result<TValue, TError>.Success(value)
            : Result<TValue, TError>.Failure(errorFactory(value)));
    }

    /// <summary>
    /// Executes a side effect for a successful result and returns the original result.
    /// </summary>
    public static Result<TValue, TError> Tap<TValue, TError>(
        this Result<TValue, TError> result,
        Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Value);
            return result;
        }

        result.EnsureInitialized();
        return result;
    }

    /// <summary>
    /// Executes a side effect for a failed result and returns the original result.
    /// </summary>
    public static Result<TValue, TError> TapError<TValue, TError>(
        this Result<TValue, TError> result,
        Action<TError> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailure)
        {
            action(result.Error);
            return result;
        }

        result.EnsureInitialized();
        return result;
    }

    /// <summary>
    /// Replaces a failed result with a recovered value.
    /// </summary>
    public static Result<TValue, TError> Recover<TValue, TError>(
        this Result<TValue, TError> result,
        Func<TError, TValue> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);

        return result.Match(
            Result<TValue, TError>.Success,
            error => Result<TValue, TError>.Success(recover(error)));
    }

    /// <summary>
    /// Replaces a failed result with another result.
    /// </summary>
    public static Result<TValue, TError> OrElse<TValue, TError>(
        this Result<TValue, TError> result,
        Func<TError, Result<TValue, TError>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return result.Match(Result<TValue, TError>.Success, fallback);
    }

    /// <summary>
    /// Converts a result to an option, discarding the failure value.
    /// </summary>
    public static Option<TValue> ToOption<TValue, TError>(
        this Result<TValue, TError> result) =>
        result.Match(Option<TValue>.Some, _ => Option<TValue>.None);

    /// <summary>
    /// Returns the success value or a fallback value.
    /// </summary>
    public static TValue GetValueOr<TValue, TError>(
        this Result<TValue, TError> result,
        TValue fallback) =>
        result.Match(value => value, _ => fallback);

    /// <summary>
    /// Returns the success value or creates a fallback value from the error.
    /// </summary>
    public static TValue GetValueOr<TValue, TError>(
        this Result<TValue, TError> result,
        Func<TError, TValue> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return result.Match(value => value, fallback);
    }

    /// <summary>
    /// Flattens a nested result.
    /// </summary>
    public static Result<TValue, TError> Flatten<TValue, TError>(
        this Result<Result<TValue, TError>, TError> result) =>
        result.Bind(static inner => inner);

    /// <summary>
    /// LINQ projection alias for <see cref="Map{TValue,TOutput,TError}" />.
    /// </summary>
    public static Result<TOutput, TError> Select<TValue, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, TOutput> selector) =>
        result.Map(selector);

    /// <summary>
    /// LINQ composition support for results.
    /// </summary>
    public static Result<TOutput, TError> SelectMany<TValue, TIntermediate, TOutput, TError>(
        this Result<TValue, TError> result,
        Func<TValue, Result<TIntermediate, TError>> bind,
        Func<TValue, TIntermediate, TOutput> project)
    {
        ArgumentNullException.ThrowIfNull(bind);
        ArgumentNullException.ThrowIfNull(project);

        return result.Bind(value => bind(value).Map(
            intermediate => project(value, intermediate)));
    }
}
