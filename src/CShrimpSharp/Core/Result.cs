using System.Diagnostics.CodeAnalysis;

namespace CShrimpSharp;

public readonly struct Result<TValue,TError>
{
    private readonly byte _state;
    private readonly TValue? _value;
    private readonly TError? _error;
    private Result(byte state, TValue? value, TError? error) { _state=state; _value=value; _error=error; }
    public bool IsInitialized => _state != 0;
    public bool IsSuccess => _state == 1;
    public bool IsFailure => _state == 2;
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Result does not contain a success value.");
    public TError Error => IsFailure ? _error! : throw new InvalidOperationException("Result does not contain an error value.");
    public static Result<TValue,TError> Success(TValue value) { ArgumentNullException.ThrowIfNull(value); return new(1,value,default); }
    public static Result<TValue,TError> Failure(TError error) { ArgumentNullException.ThrowIfNull(error); return new(2,default,error); }
    public TResult Match<TResult>(Func<TValue,TResult> success, Func<TError,TResult> failure) { EnsureInitialized(); return IsSuccess ? success(_value!) : failure(_error!); }
    public void Switch(Action<TValue> success, Action<TError> failure) { EnsureInitialized(); if(IsSuccess) success(_value!); else failure(_error!); }
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value) { EnsureInitialized(); value=_value; return IsSuccess; }
    internal void EnsureInitialized() { if(!IsInitialized) throw new InvalidOperationException("Result is uninitialized."); }
}

public static class Result
{
    public static Result<T,Failure> Success<T>(T value) => Result<T,Failure>.Success(value);
    public static Result<Unit,Failure> Success() => Result<Unit,Failure>.Success(Unit.Value);
    public static Result<T,Failure> Failure<T>(Failure error) => Result<T,Failure>.Failure(error);
    public static Result<T,Failure> Try<T>(Func<T> action)
    {
        try { return Success(action()); }
        catch(OperationCanceledException) { throw; }
        catch(Exception ex) when (ex is not OutOfMemoryException and not AccessViolationException) { return Failure<T>(CShrimpSharp.Failure.FromException(ex)); }
    }
}
