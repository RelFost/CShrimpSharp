# Architecture and guarantees

CShrimpSharp is intentionally small and dependency-light.

## Guarantees

- `Option<T>` never intentionally contains null.
- `Result<TValue,TError>` must be explicitly initialized.
- Mapping and binding preserve inactive branches.
- `SyncAsync` preserves result ordering.
- `SyncSettledAsync` preserves input ordering and captures ordinary exceptions.
- `RaceAsync` observes child tasks before returning.
- transaction compensations execute in LIFO order.
- disposal rolls back an active transaction.

## Non-goals

- Verse source compatibility;
- a scheduler or actor runtime;
- distributed transaction guarantees;
- replacing exceptions for programmer errors and cancellation.
