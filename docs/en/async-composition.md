# Asynchronous composition

Async extensions preserve the success/none semantics and execute callbacks only for active values.

## Result.MapAsync

```csharp
Result<int, Failure> source = Result.Success(21);

Result<string, Failure> text = await source.MapAsync(
    async (value, token) =>
    {
        await Task.Delay(10, token);
        return $"value:{value}";
    });
```

## Result.BindAsync

```csharp
Result<User, Failure> user = await Result.Success(userId).BindAsync(
    async (id, token) =>
    {
        User? loaded = await repository.FindAsync(id, token);
        return loaded is null
            ? Result<User, Failure>.Failure(new Failure("not_found", "User was not found."))
            : Result<User, Failure>.Success(loaded);
    });
```

## Option.MapAsync and BindAsync

```csharp
Option<string> token = Option.Some("abc");
Option<int> length = await token.MapAsync(
    static (value, _) => ValueTask.FromResult(value.Length));
```

Cancellation is passed to the callback. CShrimpSharp does not swallow `OperationCanceledException`.
