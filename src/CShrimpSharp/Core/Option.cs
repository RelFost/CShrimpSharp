using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>
/// Represents an optional non-null value.
/// </summary>
/// <typeparam name="T">The contained value type.</typeparam>
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;

    private Option(T value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>
    /// Gets a value indicating whether this option contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets a value indicating whether this option contains a value.
    /// </summary>
    public bool IsSome => HasValue;

    /// <summary>
    /// Gets a value indicating whether this option is empty.
    /// </summary>
    public bool IsNone => !HasValue;

    /// <summary>
    /// Gets the contained value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The option is empty.</exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Option contains no value.");

    /// <summary>
    /// Gets an empty option.
    /// </summary>
    public static Option<T> None => default;

    /// <summary>
    /// Creates an option containing a non-null value.
    /// </summary>
    /// <param name="value">The value to contain.</param>
    /// <returns>An option containing <paramref name="value" />.</returns>
    public static Option<T> Some(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Option<T>(value);
    }

    /// <summary>
    /// Attempts to get the contained value.
    /// </summary>
    /// <param name="value">The contained value when present.</param>
    /// <returns><see langword="true" /> when this option contains a value.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    /// <summary>
    /// Produces a value from either the present or empty branch.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return HasValue ? some(_value!) : none();
    }

    /// <inheritdoc />
    public bool Equals(Option<T> other) =>
        HasValue == other.HasValue &&
        (!HasValue || EqualityComparer<T>.Default.Equals(_value!, other._value!));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HasValue ? HashCode.Combine(true, _value) : 0;

    /// <inheritdoc />
    public override string ToString() => HasValue ? $"Some({_value})" : "None";

    /// <summary>
    /// Determines whether two options are equal.
    /// </summary>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two options are not equal.
    /// </summary>
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

/// <summary>
/// Provides factory methods for creating optional values.
/// </summary>
public static class Option
{
    /// <summary>
    /// Creates an option containing a non-null value.
    /// </summary>
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    public static Option<T> None<T>() => Option<T>.None;

    /// <summary>
    /// Creates an option that is empty when <paramref name="value" /> is null.
    /// </summary>
    public static Option<T> From<T>(T? value) =>
        value is null ? Option<T>.None : Option<T>.Some(value);
}
