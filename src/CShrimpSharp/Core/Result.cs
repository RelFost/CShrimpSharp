using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

/// <summary>
/// Represents either a successful value or an expected failure.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
/// <typeparam name="TError">The failure value type.</typeparam>
public readonly struct Result<TValue, TError>
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

    /// <summary>
    /// Gets a value indicating whether this result was explicitly initialized.
    /// </summary>
    public bool IsInitialized => _state != 0;

    /// <summary>
    /// Gets a value indicating whether this result is successful.
    /// </summary>
    public bool IsSuccess => _state == 1;

    /// <summary>
    /// Gets a value indicating whether this result contains an error.
    /// </summary>
    public bool IsFailure => _state == 2;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">This result is not successful.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Result does not contain a success value.");

    /// <summary>
    /// Gets the failure value.
    /// </summary>
    /// <exception cref="InvalidOperationException">This result is not a failure.</exception>
    public TError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Result does not contain an error value.");

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<TValue, TError> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue, TError>(1, value, default);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result<TValue, TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue, TError>(2, default, error);
    }

    /// <summary>
    /// Produces one value from either result branch.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        EnsureInitialized();
        return IsSuccess ? success(_value!) : failure(_error!);
    }

    /// <summary>
    /// Executes one action for the active result branch.
    /// </summary>
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

    /// <summary>
    /// Attempts to get the successful value.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        EnsureInitialized();
        value = _value;
        return IsSuccess;
    }

    internal void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Result is uninitialized.");
        }
    }
}

/// <summary>
/// Provides factory methods for common result values.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result using <see cref="Failure" /> as its error type.
    /// </summary>
    public static Result<T, Failure> Success<T>(T value) => Result<T, Failure>.Success(value);

    /// <summary>
    /// Creates a successful unit result.
    /// </summary>
    public static Result<Unit, Failure> Success() =>
        Result<Unit, Failure>.Success(Unit.Value);

    /// <summary>
    /// Creates a failed result using <see cref="Failure" /> as its error type.
    /// </summary>
    public static Result<T, Failure> Failure<T>(Failure error) =>
        Result<T, Failure>.Failure(error);

    /// <summary>
    /// Executes code and converts ordinary exceptions into <see cref="Failure" /> values.
    /// Cancellation and fatal runtime exceptions remain exceptions.
    /// </summary>
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
