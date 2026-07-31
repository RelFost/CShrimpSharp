# Architecture

## Goals

CShrimpSharp provides a small set of ordinary C# primitives inspired by selected Verse concepts. The library should remain understandable without generated code, a custom runtime, or hidden global schedulers.

The preview has four architectural areas:

1. **Core values** — `Result`, `Option`, `Failure`, and `Unit`.
2. **Composition extensions** — synchronous, asynchronous, collection, and LINQ helpers.
3. **Structured concurrency** — `SyncAsync`, `RaceAsync`, and owned branch scopes.
4. **Explicit compensation** — reversible steps registered in `ShrimpTransaction`.

## Package layout

The preview publishes one assembly and one NuGet package:

```text
CShrimpSharp.dll
```

Public namespaces are already separated:

```text
CShrimpSharp
CShrimpSharp.Concurrency
CShrimpSharp.Transactions
```

This keeps installation simple while allowing a later physical split into packages such as `CShrimpSharp.Concurrency`, `CShrimpSharp.Transactions`, and `CShrimpSharp.Analyzers` without renaming public types.

## Core values

### `Result<TValue, TError>`

`Result` is a readonly struct with three internal states:

```text
Uninitialized
Success
Failure
```

The explicit uninitialized state is intentional. A default-created struct must not silently behave like a valid success or failure. Accessing `Value`, `Error`, or a composition operation on an uninitialized result raises `InvalidResultAccessException`.

Success and error payloads are required to be non-null. Optional data should be represented explicitly through `Option<TValue>` instead of storing null in an active result branch.

### `Option<TValue>`

`Option` is a readonly struct whose default value is `None`. `Some` rejects null. This gives a cheap and predictable optional value with no separate allocation.

### Composition

Composition methods preserve the inactive branch:

- `Map` changes only success values;
- `MapError` changes only errors;
- `Bind` sequences operations that may fail;
- `Ensure` validates a success;
- `Sequence` and `Traverse` stop at the first failure;
- LINQ support delegates to `Map` and `Bind`.

No composition method catches user exceptions. Use `Result.Try` or `Result.TryAsync` at an explicit exception boundary.

## Structured concurrency

### Ownership model

A structured operation owns all child operations that it starts. Public methods therefore accept delegates rather than already-running `Task` instances. The owner creates the tasks with one linked cancellation token and observes every task before returning.

### `SyncAsync`

`SyncAsync` starts all supplied delegates, preserves input ordering for values, and awaits all operations. A non-cancellation failure requests cancellation of sibling operations. The final exception behavior remains the behavior of `Task.WhenAll`.

### `RaceAsync`

`RaceAsync` starts all delegates and uses the first completed task as the winner, regardless of whether it succeeds, fails, or is cancelled. It then:

1. requests cancellation for all operations;
2. awaits the winning task to determine the public outcome;
3. observes every losing task before returning or throwing.

This prevents unobserved child failures. It also means a losing operation that ignores cancellation can delay completion indefinitely. Forced task termination is intentionally not attempted.

### `ShrimpScope`

A scope owns explicitly registered branches. Branch registration and sealing are synchronized so `JoinAsync` cannot miss a branch being registered concurrently. Once joining starts, the scope rejects new branches.

By default, an ordinary branch failure requests cancellation of sibling branches. The scope still observes all branches before propagating failure. Concurrent disposal callers share the same cleanup operation and do not return before owned branches have been drained.

## Compensating transactions

`ShrimpTransaction` stores compensation delegates in a stack. A rollback:

1. seals the active transaction;
2. snapshots and clears the stack;
3. invokes every compensation from newest to oldest;
4. aggregates compensation failures after all actions have been attempted.

`RunAsync` commits on a successful result and rolls back on a failed result or exception. Rollback uses `CancellationToken.None` in this helper so cancellation of the original operation does not automatically abandon compensation. If a failed result triggers a rollback that itself fails, `TransactionRollbackException` is thrown because the failed result cannot be returned safely. If execution throws and rollback also fails, an `AggregateException` preserves both causes.

This is a local compensation mechanism, not isolation, atomic memory rollback, a database transaction, or a distributed transaction coordinator.

## Error and cancellation rules

- Expected domain failures should use `Result`.
- Programmer errors and broken invariants should throw.
- `OperationCanceledException` remains cancellation and is not converted to `Failure` by `Result.Try`.
- Sibling cancellation is cooperative.
- A cancellation callback must not replace the original branch failure; internal cancellation helpers therefore suppress callback aggregation when necessary.

## Thread-safety boundaries

`ShrimpScope` supports concurrent branch registration only before joining begins. `ShrimpTransaction` protects state and compensation registration, but transaction steps are intended to be orchestrated sequentially. Concurrent external mutation of the same state remains the caller's responsibility.

## Future analyzers

A separate Roslyn analyzer package can add diagnostics that a runtime library cannot enforce, including:

- suspicious direct `Value` or `Error` access;
- transaction operations without nearby compensation;
- delegates that discard a supplied cancellation token;
- branch registration after an awaited operation in a scope body;
- API calls whose failure or transaction semantics are ambiguous.

Analyzers must remain optional and must not be required to execute the runtime package.
