using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>
/// Represents either a successful value or a failure value.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
/// <typeparam name="TError">The failure value type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
{
    private readonly ResultState _state;
    private readonly TValue? _value;
    private readonly TError? _error;

    private Result(ResultState state, TValue? value, TError? error)
    {
        _state = state;
        _value = value;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the result has been explicitly initialized.
    /// </summary>
    public bool IsInitialized => _state is not ResultState.Uninitialized;

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess => _state is ResultState.Success;

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => _state is ResultState.Failure;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidResultAccessException">
    /// The result is uninitialized or contains a failure.
    /// </exception>
    public TValue Value => _state switch
    {
        ResultState.Success => _value!,
        ResultState.Failure => throw new InvalidResultAccessException(
            "Cannot read Value from a failed result. Read Error or use Match instead."),
        _ => throw new InvalidResultAccessException(
            "Cannot read Value from an uninitialized result. Create it with Result.Success or Result.Failure."),
    };

    /// <summary>
    /// Gets the failure value.
    /// </summary>
    /// <exception cref="InvalidResultAccessException">
    /// The result is uninitialized or contains a success.
    /// </exception>
    public TError Error => _state switch
    {
        ResultState.Failure => _error!,
        ResultState.Success => throw new InvalidResultAccessException(
            "Cannot read Error from a successful result. Read Value or use Match instead."),
        _ => throw new InvalidResultAccessException(
            "Cannot read Error from an uninitialized result. Create it with Result.Success or Result.Failure."),
    };

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result.</returns>
    public static Result<TValue, TError> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TError>(ResultState.Success, value, default);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The failure value.</param>
    /// <returns>A failed result.</returns>
    public static Result<TValue, TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue, TError>(ResultState.Failure, default, error);
    }

    /// <summary>
    /// Attempts to get the success value.
    /// </summary>
    /// <param name="value">The success value when present.</param>
    /// <returns><see langword="true" /> when the result is successful.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        EnsureInitialized();

        if (IsSuccess)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the failure value.
    /// </summary>
    /// <param name="error">The failure value when present.</param>
    /// <returns><see langword="true" /> when the result is a failure.</returns>
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        EnsureInitialized();

        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        error = default;
        return false;
    }

    /// <summary>
    /// Produces one value from either result branch.
    /// </summary>
    /// <typeparam name="TResult">The produced value type.</typeparam>
    /// <param name="onSuccess">Called for a successful result.</param>
    /// <param name="onFailure">Called for a failed result.</param>
    /// <returns>The value produced by the selected branch.</returns>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<TError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        EnsureInitialized();

        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>
    /// Executes one action for the active result branch.
    /// </summary>
    /// <param name="onSuccess">Called for a successful result.</param>
    /// <param name="onFailure">Called for a failed result.</param>
    public void Switch(Action<TValue> onSuccess, Action<TError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        EnsureInitialized();

        if (IsSuccess)
        {
            onSuccess(_value!);
            return;
        }

        onFailure(_error!);
    }

    /// <inheritdoc />
    public bool Equals(Result<TValue, TError> other)
    {
        if (_state != other._state)
        {
            return false;
        }

        return _state switch
        {
            ResultState.Uninitialized => true,
            ResultState.Success => EqualityComparer<TValue>.Default.Equals(_value!, other._value!),
            ResultState.Failure => EqualityComparer<TError>.Default.Equals(_error!, other._error!),
            _ => false,
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Result<TValue, TError> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _state switch
    {
        ResultState.Success => HashCode.Combine(_state, _value),
        ResultState.Failure => HashCode.Combine(_state, _error),
        _ => 0,
    };

    /// <inheritdoc />
    public override string ToString() => _state switch
    {
        ResultState.Success => $"Success({_value})",
        ResultState.Failure => $"Failure({_error})",
        _ => "Uninitialized",
    };

    /// <summary>
    /// Determines whether two results are equal.
    /// </summary>
    public static bool operator ==(
        Result<TValue, TError> left,
        Result<TValue, TError> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two results are not equal.
    /// </summary>
    public static bool operator !=(
        Result<TValue, TError> left,
        Result<TValue, TError> right) => !left.Equals(right);

    internal void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidResultAccessException(
                "The result is uninitialized. Create it with Result.Success or Result.Failure before using it.");
        }
    }

    private string DebuggerDisplay => ToString();
}

/// <summary>
/// Factory methods for creating common result values.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result using <see cref="Failure" /> as the error type.
    /// </summary>
    public static Result<TValue, Failure> Success<TValue>(TValue value) =>
        Result<TValue, Failure>.Success(value);

    /// <summary>
    /// Creates a successful unit result.
    /// </summary>
    public static Result<Unit, Failure> Success() =>
        Result<Unit, Failure>.Success(Unit.Value);

    /// <summary>
    /// Creates a successful result with an explicit error type.
    /// </summary>
    public static Result<TValue, TError> Success<TValue, TError>(TValue value) =>
        Result<TValue, TError>.Success(value);

    /// <summary>
    /// Creates a failed result using <see cref="Failure" /> as the error type.
    /// </summary>
    public static Result<TValue, Failure> Failure<TValue>(Failure failure) =>
        Result<TValue, Failure>.Failure(failure);

    /// <summary>
    /// Creates a failed result with an explicit error type.
    /// </summary>
    public static Result<TValue, TError> Failure<TValue, TError>(TError error) =>
        Result<TValue, TError>.Failure(error);

    /// <summary>
    /// Executes synchronous code and converts non-cancellation exceptions into failures.
    /// </summary>
    public static Result<TValue, Failure> Try<TValue>(
        Func<TValue> action,
        Func<Exception, Failure>? mapException = null)
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
        {
            Failure failure = mapException is null
                ? CShrimpSharp.Failure.FromException(exception)
                : mapException(exception);

            return Failure<TValue>(failure);
        }
    }

    /// <summary>
    /// Executes asynchronous code and converts non-cancellation exceptions into failures.
    /// </summary>
    public static async ValueTask<Result<TValue, Failure>> TryAsync<TValue>(
        Func<CancellationToken, ValueTask<TValue>> action,
        Func<Exception, Failure>? mapException = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            TValue value = await action(cancellationToken).ConfigureAwait(false);
            return Success(value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Failure failure = mapException is null
                ? CShrimpSharp.Failure.FromException(exception)
                : mapException(exception);

            return Failure<TValue>(failure);
        }
    }
}
