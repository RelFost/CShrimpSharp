using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value;
    private Option(T value) { _value = value; HasValue = true; }
    public bool HasValue { get; }
    public bool IsSome => HasValue;
    public bool IsNone => !HasValue;
    public T Value => HasValue ? _value! : throw new InvalidOperationException("Option contains no value.");
    public static Option<T> None => default;
    public static Option<T> Some(T value) { ArgumentNullException.ThrowIfNull(value); return new(value); }
    public bool TryGetValue([MaybeNullWhen(false)] out T value) { value = _value; return HasValue; }
    public TResult Match<TResult>(Func<T,TResult> some, Func<TResult> none) => HasValue ? some(_value!) : none();
    public bool Equals(Option<T> other) => HasValue == other.HasValue && (!HasValue || EqualityComparer<T>.Default.Equals(_value!, other._value!));
    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);
    public override int GetHashCode() => HasValue ? HashCode.Combine(true, _value) : 0;
}

public static class Option
{
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);
    public static Option<T> None<T>() => Option<T>.None;
    public static Option<T> From<T>(T? value) => value is null ? Option<T>.None : Option<T>.Some(value);
}
