# Cookbook

## Convert nullable repository output

```csharp
Option<User> user = Option.From(await repository.FindAsync(id, token));
```

## Convert an option to a domain failure

```csharp
Result<User, Failure> requiredUser = user.ToResult(
    static () => new Failure("user_not_found", "The requested user does not exist."));
```

## Apply a default only for expected failure

```csharp
int retryCount = configurationResult.GetValueOrElse(
    error => error.Code == "missing" ? 3 : 0);
```

## Query several optional providers

```csharp
RaceResult<Token> token = await Shrimp.RaceSuccessAsync(
    LoadFromMemoryAsync,
    LoadFromDiskAsync,
    RefreshFromServerAsync);
```

## Roll back resources automatically

```csharp
await using var transaction = new ShrimpTransaction();
Resource resource = await transaction.StepAsync(CreateAsync, DeleteAsync, token);
await UseAsync(resource, token);
transaction.Commit();
```
