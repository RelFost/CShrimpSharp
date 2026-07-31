namespace CShrimpSharp;

/// <summary>
/// Functional composition helpers for <see cref="Option{TValue}" />.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Maps a present value while preserving an empty option.
    /// </summary>
    public static Option<TOutput> Map<TValue, TOutput>(
        this Option<TValue> option,
        Func<TValue, TOutput> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return option.Match(
            value => Option<TOutput>.Some(map(value)),
            () => Option<TOutput>.None);
    }

    /// <summary>
    /// Chains an option-producing operation after a present value.
    /// </summary>
    public static Option<TOutput> Bind<TValue, TOutput>(
        this Option<TValue> option,
        Func<TValue, Option<TOutput>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return option.Match(bind, () => Option<TOutput>.None);
    }

    /// <summary>
    /// Keeps a present value only when the predicate is true.
    /// </summary>
    public static Option<TValue> Filter<TValue>(
        this Option<TValue> option,
        Func<TValue, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return option.Match(
            value => predicate(value) ? option : Option<TValue>.None,
            () => Option<TValue>.None);
    }

    /// <summary>
    /// Executes a side effect for a present value and returns the original option.
    /// </summary>
    public static Option<TValue> Tap<TValue>(
        this Option<TValue> option,
        Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (option.HasValue)
        {
            action(option.Value);
        }

        return option;
    }

    /// <summary>
    /// Returns the original option or a lazily created fallback.
    /// </summary>
    public static Option<TValue> OrElse<TValue>(
        this Option<TValue> option,
        Func<Option<TValue>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.HasValue ? option : fallback();
    }

    /// <summary>
    /// Converts an option to a result.
    /// </summary>
    public static Result<TValue, TError> ToResult<TValue, TError>(
        this Option<TValue> option,
        Func<TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);

        return option.Match(
            Result<TValue, TError>.Success,
            () => Result<TValue, TError>.Failure(errorFactory()));
    }

    /// <summary>
    /// Returns the present value or a fallback value.
    /// </summary>
    public static TValue GetValueOr<TValue>(
        this Option<TValue> option,
        TValue fallback) =>
        option.Match(value => value, () => fallback);

    /// <summary>
    /// Returns the present value or a lazily created fallback value.
    /// </summary>
    public static TValue GetValueOr<TValue>(
        this Option<TValue> option,
        Func<TValue> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.Match(value => value, fallback);
    }

    /// <summary>
    /// Flattens a nested option.
    /// </summary>
    public static Option<TValue> Flatten<TValue>(
        this Option<Option<TValue>> option) =>
        option.Bind(static inner => inner);

    /// <summary>
    /// LINQ projection alias for <see cref="Map{TValue,TOutput}" />.
    /// </summary>
    public static Option<TOutput> Select<TValue, TOutput>(
        this Option<TValue> option,
        Func<TValue, TOutput> selector) =>
        option.Map(selector);

    /// <summary>
    /// LINQ composition support for options.
    /// </summary>
    public static Option<TOutput> SelectMany<TValue, TIntermediate, TOutput>(
        this Option<TValue> option,
        Func<TValue, Option<TIntermediate>> bind,
        Func<TValue, TIntermediate, TOutput> project)
    {
        ArgumentNullException.ThrowIfNull(bind);
        ArgumentNullException.ThrowIfNull(project);

        return option.Bind(value => bind(value).Map(
            intermediate => project(value, intermediate)));
    }
}
