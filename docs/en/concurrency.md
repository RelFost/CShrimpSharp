# Structured concurrency

## SyncAsync

Starts operations together and returns values in declaration order.

```csharp
(string profile, int count, bool enabled) = await Shrimp.SyncAsync(
    LoadProfileAsync,
    LoadCountAsync,
    LoadFeatureFlagAsync,
    cancellationToken);
```

Typed overloads are available for two through five operations. For a homogeneous dynamic collection use the enumerable overload.

## SyncSettledAsync

Use this when every result must be collected, including failures:

```csharp
IReadOnlyList<Result<string, Exception>> results = await Shrimp.SyncSettledAsync<string>(
[
    LoadPrimaryAsync,
    LoadSecondaryAsync,
    LoadCacheAsync,
], cancellationToken);

foreach (Result<string, Exception> result in results)
{
    result.Switch(Console.WriteLine, error => Console.WriteLine(error.Message));
}
```

## RaceAsync

Returns the first completed operation. The winner may be successful or failed.

```csharp
RaceResult<string> winner = await Shrimp.RaceAsync(
    LoadFromRegionAAsync,
    LoadFromRegionBAsync);
```

## RaceSuccessAsync

Ignores failed operations until one succeeds. If all fail, it throws `AggregateException`.

```csharp
RaceResult<string> winner = await Shrimp.RaceSuccessAsync(
    LoadFromPrimaryAsync,
    LoadFromReplicaAsync,
    LoadFromArchiveAsync);
```

## Timeouts

```csharp
string response = await Shrimp.WithTimeoutAsync(
    FetchAsync,
    TimeSpan.FromSeconds(2),
    cancellationToken);
```

External cancellation remains `OperationCanceledException`; an elapsed timeout becomes `TimeoutException`.
