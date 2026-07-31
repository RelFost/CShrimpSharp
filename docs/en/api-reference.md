# API reference at a glance

This page is a compact map of the public API in `0.3.0-preview.1`. The focused guides explain behavior and edge cases in more detail.

## Core values

| API | Purpose | Typical use |
| --- | --- | --- |
| `Option.Some(value)` | Create a present optional value. | A lookup returned a non-null value. |
| `Option.None<T>()` | Create an empty optional value. | Absence is expected and needs no error details. |
| `Option.From(value)` | Convert a nullable reference to an option. | Environment variables and optional configuration. |
| `Result.Success(value)` | Create a successful result using `Failure`. | Start an explicit success/failure pipeline. |
| `Result<T,E>.Success(value)` | Create a success with a custom error type. | Domain-specific errors. |
| `Result<T,E>.Failure(error)` | Create a failure. | Expected validation, lookup, or business failure. |
| `Result.Try(action)` | Convert ordinary exceptions into `Failure`. | Parsing or boundary code where exceptions are expected. |
| `Validation<T,E>.Valid(value)` | Create a valid value. | Independent input validation. |
| `Validation<T,E>.Invalid(errors)` | Accumulate validation errors. | Forms, configuration, and batch validation. |

## Composition

```csharp
Result<int, Failure> result = Result
    .Try(() => int.Parse("42", CultureInfo.InvariantCulture))
    .Ensure(
        static value => value > 0,
        static value => new Failure("not_positive", $"Expected positive, got {value}."))
    .Map(static value => value * 2)
    .Tap(static value => Console.WriteLine($"Computed {value}"));
```

| Method | Active branch | Result |
| --- | --- | --- |
| `Map` | success / Some | Transformed value. |
| `Bind` | success / Some | Flattened result from the next operation. |
| `MapError` | failure | Transformed error type. |
| `Ensure` | success | Original success or a new failure. |
| `Recover` | failure | Successful fallback value. |
| `RecoverWith` | failure | Replacement result. |
| `Tap` / `TapError` | matching branch | Original result after a side effect. |
| `GetValueOr` | either | Value or eager fallback. |
| `GetValueOrElse` | either | Value or lazy fallback. |
| `ToOption` / `ToResult` | either | Conversion between optional and error-aware values. |

Async equivalents are available through `MapAsync`, `BindAsync`, and `MatchAsync`.

## Structured concurrency

```csharp
(string profile, int count, bool online) = await Shrimp.SyncAsync(
    LoadProfileAsync,
    LoadCountAsync,
    CheckOnlineAsync,
    cancellationToken);
```

| API | Completion rule | Failure behavior |
| --- | --- | --- |
| `SyncAsync` | All operations complete. | A failure requests sibling cancellation and propagates. |
| `SyncSettledAsync` | All operations settle. | Returns each exception as `Result<T,Exception>`. |
| `RaceAsync` | First completion wins. | A fast failure is a valid winner and propagates. |
| `RaceSuccessAsync` | First success wins. | Throws `AggregateException` when every contender fails. |
| `WithTimeoutAsync` | Operation completes before timeout. | Throws `TimeoutException`; caller cancellation stays cancellation. |

Typed `SyncAsync` overloads are provided for two through five differently typed operations.

## Compensating transactions

```csharp
await using var transaction = new ShrimpTransaction();

string reservationId = await transaction.StepAsync(
    ReserveAsync,
    static (id, token) => ReleaseAsync(id, token),
    cancellationToken);

transaction.Commit();
```

| Member | Meaning |
| --- | --- |
| `OnRollback` | Register a synchronous or asynchronous compensation. |
| `StepAsync` | Execute an operation, then register compensation after success. |
| `StepAsync<T>` | Register compensation that receives the operation result. |
| `Commit` | Discard compensations and mark the transaction committed. |
| `RollbackAsync` | Execute compensations in reverse order. |
| `State` | Observe `Active`, `Committed`, `RolledBack`, or `RollbackFailed`. |

## Safe collections

```csharp
Option<string> second = values.AtOrNone(1);
Option<User> user = users.FindOrNone(userId);
Result<User, Failure> only = candidates.SingleResult();
```

Use these helpers when a missing index, key, or unique element is an ordinary domain outcome rather than an exceptional programming error.
