# Практические рецепты

## Nullable-результат репозитория в Option

```csharp
Option<User> user = Option.From(await repository.FindAsync(id, token));
```

## Option в доменную ошибку

```csharp
Result<User, Failure> requiredUser = user.ToResult(
    static () => new Failure("user_not_found", "Пользователь не существует."));
```

## Значение по умолчанию для ожидаемой ошибки

```csharp
int retryCount = configurationResult.GetValueOrElse(
    error => error.Code == "missing" ? 3 : 0);
```

## Несколько источников с первым успешным ответом

```csharp
RaceResult<Token> token = await Shrimp.RaceSuccessAsync(
    LoadFromMemoryAsync,
    LoadFromDiskAsync,
    RefreshFromServerAsync);
```

## Автоматический rollback ресурсов

```csharp
await using var transaction = new ShrimpTransaction();
Resource resource = await transaction.StepAsync(CreateAsync, DeleteAsync, token);
await UseAsync(resource, token);
transaction.Commit();
```
