using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>
/// Represents either a successful value or an expected failure.
/// </summary>
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

    /// <summary>Gets whether this result was explicitly initialized.</summary>
    public bool IsInitialized => _state != 0;

    /// <summary>Gets whether this result is successful.</summary>
    public bool IsSuccess => _state == 1;

    /// <summary>Gets whether this result contains an error.</summary>
    public bool IsFailure => _state == 2;

    /// <summary>Gets the successful value.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Result does not contain a success value.");

    /// <summary>Gets the failure value.</summary>
    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Result does not contain an error value.");

    /// <summary>Creates a successful result.</summary>
    public static Result<TValue, TError> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TError>(1, value, default);
    }

    /// <summary>Creates a failed result.</summary>
    public static Result<TValue, TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue, TError>(2, default, error);
    }

    /// <summary>Produces one value from either result branch.</summary>
    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        EnsureInitialized();
        return IsSuccess ? success(_value!) : failure(_error!);
    }

    /// <summary>Executes one action for the active result branch.</summary>
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

    /// <summary>Attempts to get the successful value.</summary>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        EnsureInitialized();
        value = _value;
        return IsSuccess;
    }

    /// <summary>Attempts to get the failure value.</summary>
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        EnsureInitialized();
        error = _error;
        return IsFailure;
    }

    /// <inheritdoc />
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
    public override bool Equals(object? obj) =>
        obj is Result<TValue, TError> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        EnsureInitialized();
        return IsSuccess
            ? HashCode.Combine(_state, _value)
            : HashCode.Combine(_state, _error);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        EnsureInitialized();
        return IsSuccess ? $"Success({_value})" : $"Failure({_error})";
    }

    /// <summary>Determines whether two results are equal.</summary>
    public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) =>
        left.Equals(right);

    /// <summary>Determines whether two results are not equal.</summary>
    public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right) =>
        !left.Equals(right);

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
    /// <summary>Creates a successful result using <see cref="Failure" /> as its error type.</summary>
    public static Result<T, Failure> Success<T>(T value) => Result<T, Failure>.Success(value);

    /// <summary>Creates a successful unit result.</summary>
    public static Result<Unit, Failure> Success() => Result<Unit, Failure>.Success(Unit.Value);

    /// <summary>Creates a failed result using <see cref="Failure" /> as its error type.</summary>
    public static Result<T, Failure> Failure<T>(Failure error) => Result<T, Failure>.Failure(error);

    /// <summary>Executes code and converts ordinary exceptions into <see cref="Failure" /> values.</summary>
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
        catch (Exception exception)
            when (exception is not OutOfMemoryException and not AccessViolationException)
        {
            return Failure<T>(CShrimpSharp.Failure.FromException(exception));
        }
    }
}
