namespace CShrimpSharp;

public static class FunctionalExtensions
{
    public static Result<TOut,TError> Map<TIn,TOut,TError>(this Result<TIn,TError> result, Func<TIn,TOut> map)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? Result<TOut,TError>.Success(map(result.Value)) : Result<TOut,TError>.Failure(result.Error);
    }
    public static Result<TOut,TError> Bind<TIn,TOut,TError>(this Result<TIn,TError> result, Func<TIn,Result<TOut,TError>> bind)
    {
        result.EnsureInitialized();
        return result.IsSuccess ? bind(result.Value) : Result<TOut,TError>.Failure(result.Error);
    }
    public static Result<T,TError> Ensure<T,TError>(this Result<T,TError> result, Func<T,bool> predicate, Func<T,TError> error)
    {
        result.EnsureInitialized();
        return result.IsFailure || predicate(result.Value) ? result : Result<T,TError>.Failure(error(result.Value));
    }
    public static Option<TOut> Map<TIn,TOut>(this Option<TIn> option, Func<TIn,TOut> map) => option.IsSome ? Option.Some(map(option.Value)) : Option.None<TOut>();
    public static Option<T> Filter<T>(this Option<T> option, Func<T,bool> predicate) => option.IsSome && predicate(option.Value) ? option : Option.None<T>();
    public static T GetValueOr<T>(this Option<T> option, T fallback) => option.IsSome ? option.Value : fallback;
}
