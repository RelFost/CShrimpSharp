# Структурированная конкурентность

## SyncAsync

Запускает операции вместе и возвращает значения в порядке объявления.

```csharp
(string profile, int count, bool enabled) = await Shrimp.SyncAsync(
    LoadProfileAsync,
    LoadCountAsync,
    LoadFeatureFlagAsync,
    cancellationToken);
```

Типизированные overload доступны для двух–пяти операций. Для динамического однотипного набора используй enumerable-overload.

## SyncSettledAsync

Собирает каждый результат, включая ошибки:

```csharp
IReadOnlyList<Result<string, Exception>> results = await Shrimp.SyncSettledAsync<string>(
[
    LoadPrimaryAsync,
    LoadSecondaryAsync,
    LoadCacheAsync,
], cancellationToken);
```

## RaceAsync

Возвращает первую завершившуюся операцию. Она может завершиться как успешно, так и ошибкой.

## RaceSuccessAsync

Пропускает ошибки до первого успешного результата. Если упали все операции, выбрасывается `AggregateException`.

```csharp
RaceResult<string> winner = await Shrimp.RaceSuccessAsync(
    LoadFromPrimaryAsync,
    LoadFromReplicaAsync,
    LoadFromArchiveAsync);
```

## Таймаут

```csharp
string response = await Shrimp.WithTimeoutAsync(
    FetchAsync,
    TimeSpan.FromSeconds(2),
    cancellationToken);
```

Внешняя отмена остаётся `OperationCanceledException`, истёкший таймаут становится `TimeoutException`.
