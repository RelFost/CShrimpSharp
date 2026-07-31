using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>Represents either a successful value or an expected failure.</summary>
/// <remarks>
/// A default-initialized result is invalid. Create values through <see cref="Success"/>,
/// <see cref="Failure"/>, or the non-generic <see cref="Result"/> factory class.
/// </remarks>
/// <example>
/// <code>
/// Result&lt;int, Failure&gt; result = Result.Success(21).Map(static value =&gt; value * 2);
/// </code>
/// </example>
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
{
    private readonly byte _state;
    private readonly TValue? _value;
    private readonly TError? _error;

    private Result(byte state, TValue? value, TError? error)
    {
        _state = state;
        _value = value;
        _error = error;
    }

    /// <summary>Gets whether this result was created through a success or failure factory.</summary>
    public bool IsInitialized => _state != 0;
    /// <summary>Gets whether this result contains a successful value.</summary>
    public bool IsSuccess => _state == 1;
    /// <summary>Gets whether this result contains an error value.</summary>
    public bool IsFailure => _state == 2;

    /// <summary>Gets whether this result contains a successful value.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Result does not contain a success value.");

    /// <summary>Gets whether this result contains an error value.</summary>
    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Result does not contain an error value.");

    /// <summary>Creates a successful result containing the supplied non-null value.</summary>
    public static Result<TValue, TError> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TError>(1, value, default);
    }

    /// <summary>Creates a failed result containing the supplied non-null error.</summary>
    public static Result<TValue, TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue, TError>(2, default, error);
    }

    /// <summary>Projects the active result branch into a single value.</summary>
    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        EnsureInitialized();
        return IsSuccess ? success(_value!) : failure(_error!);
    }

    /// <summary>Executes exactly one action for the active result branch.</summary>
    public void Switch(Action<TValue> success, Action<TError> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        EnsureInitialized();

        if (IsSuccess)
        {
            success(_value!);
        }
        else
        {
            failure(_error!);
        }
    }

    /// <summary>Attempts to retrieve the successful value without throwing for a failure.</summary>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        EnsureInitialized();
        value = _value;
        return IsSuccess;
    }

    /// <summary>Attempts to retrieve the error value without throwing for a success.</summary>
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        EnsureInitialized();
        error = _error;
        return IsFailure;
    }

    /// <summary>Determines whether this result and another initialized result contain equal branches and values.</summary>
    public bool Equals(Result<TValue, TError> other)
    {
        EnsureInitialized();
        other.EnsureInitialized();

        return _state == other._state &&
            (IsSuccess
                ? EqualityComparer<TValue>.Default.Equals(_value!, other._value!)
                : EqualityComparer<TError>.Default.Equals(_error!, other._error!));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<TValue, TError> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        EnsureInitialized();
        return IsSuccess ? HashCode.Combine(_state, _value) : HashCode.Combine(_state, _error);
    }

    /// <summary>Returns a diagnostic representation of the active branch and its value.</summary>
    public override string ToString()
    {
        EnsureInitialized();
        return IsSuccess ? $"Success({_value})" : $"Failure({_error})";
    }

    /// <summary>Determines whether two initialized results are equal.</summary>
    public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);
    /// <summary>Determines whether two initialized results are not equal.</summary>
    public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right) => !left.Equals(right);

    internal void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Result is uninitialized.");
        }
    }
}

/// <summary>Provides factory methods for common result values.</summary>
public static class Result
{
    /// <summary>Creates a successful result using <see cref="Failure"/> as its error type.</summary>
    public static Result<T, Failure> Success<T>(T value) => Result<T, Failure>.Success(value);
    /// <summary>Creates a successful result containing the supplied non-null value.</summary>
    public static Result<Unit, Failure> Success() => Result<Unit, Failure>.Success(Unit.Value);
    /// <summary>Creates a failed result using <see cref="Failure"/> as its error type.</summary>
    public static Result<T, Failure> Failure<T>(Failure error) => Result<T, Failure>.Failure(error);

    /// <summary>Executes an operation and converts ordinary exceptions into <see cref="Failure"/> values.</summary>
    /// <remarks>Cancellation and fatal runtime exceptions are not converted.</remarks>
    public static Result<T, Failure> Try<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            return Success(action());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not AccessViolationException)
        {
            return Failure<T>(CShrimpSharp.Failure.FromException(exception));
        }
    }
}
