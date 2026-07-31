# Асинхронная композиция

Асинхронные расширения сохраняют семантику успеха и отсутствия: callback вызывается только для активной ветки.

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
            ? Result<User, Failure>.Failure(new Failure("not_found", "Пользователь не найден."))
            : Result<User, Failure>.Success(loaded);
    });
```

## Option.MapAsync и BindAsync

```csharp
Option<string> token = Option.Some("abc");
Option<int> length = await token.MapAsync(
    static (value, _) => ValueTask.FromResult(value.Length));
```

Отмена передаётся callback-функции. `OperationCanceledException` библиотека не скрывает.
