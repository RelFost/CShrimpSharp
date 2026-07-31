using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>
/// Represents an optional non-null value.
/// </summary>
/// <typeparam name="TValue">The contained value type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Option<TValue> : IEquatable<Option<TValue>>
{
    private readonly TValue? _value;

    private Option(TValue value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>
    /// Gets a value indicating whether the option contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets a value indicating whether the option contains a value.
    /// </summary>
    public bool IsSome => HasValue;

    /// <summary>
    /// Gets a value indicating whether the option is empty.
    /// </summary>
    public bool IsNone => !HasValue;

    /// <summary>
    /// Gets the contained value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The option is empty.</exception>
    public TValue Value => HasValue
        ? _value!
        : throw new InvalidOperationException(
            "Cannot read Value from an empty option. Use Match, TryGetValue, or provide a fallback.");

    /// <summary>
    /// Gets an empty option.
    /// </summary>
    public static Option<TValue> None => default;

    /// <summary>
    /// Creates an option containing a non-null value.
    /// </summary>
    public static Option<TValue> Some(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Option<TValue>(value);
    }

    /// <summary>
    /// Attempts to get the contained value.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        if (HasValue)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Produces one value from either option branch.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSome,
        Func<TResult> onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        return HasValue ? onSome(_value!) : onNone();
    }

    /// <summary>
    /// Executes one action for the active option branch.
    /// </summary>
    public void Switch(Action<TValue> onSome, Action onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        if (HasValue)
        {
            onSome(_value!);
            return;
        }

        onNone();
    }

    /// <inheritdoc />
    public bool Equals(Option<TValue> other) =>
        HasValue == other.HasValue &&
        (!HasValue || EqualityComparer<TValue>.Default.Equals(_value!, other._value!));

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Option<TValue> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HasValue ? HashCode.Combine(true, _value) : 0;

    /// <inheritdoc />
    public override string ToString() =>
        HasValue ? $"Some({_value})" : "None";

    /// <summary>
    /// Determines whether two options are equal.
    /// </summary>
    public static bool operator ==(Option<TValue> left, Option<TValue> right) =>
        left.Equals(right);

    /// <summary>
    /// Determines whether two options are not equal.
    /// </summary>
    public static bool operator !=(Option<TValue> left, Option<TValue> right) =>
        !left.Equals(right);

    private string DebuggerDisplay => ToString();
}

/// <summary>
/// Factory methods for creating option values.
/// </summary>
public static class Option
{
    /// <summary>
    /// Creates an option containing a non-null value.
    /// </summary>
    public static Option<TValue> Some<TValue>(TValue value) =>
        Option<TValue>.Some(value);

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    public static Option<TValue> None<TValue>() =>
        Option<TValue>.None;

    /// <summary>
    /// Creates an option that is empty when <paramref name="value" /> is null.
    /// </summary>
    public static Option<TValue> From<TValue>(TValue? value) =>
        value is null ? Option<TValue>.None : Option<TValue>.Some(value);
}
