# Design principles

## 1. Explicit behavior beats imitation

CShrimpSharp should capture useful behavior inspired by Verse without pretending that a C# library can reproduce Verse syntax, effect checking, deterministic scheduling, or automatic rollback.

## 2. Child work has an owner

An operation that creates asynchronous children must define when they are joined, when cancellation is requested, and how their failures are observed.

## 3. Expected failure is data

`Result` is for expected domain outcomes. Exceptions remain appropriate for invalid API use, violated invariants, unavailable runtime resources, and unexpected failures.

## 4. Cancellation is cooperative

Every concurrency primitive passes one `CancellationToken` to its children. The library never uses thread abortion or attempts to forcibly stop arbitrary user code.

## 5. Rollback is explicit

A library cannot reverse an arbitrary assignment or external side effect. Every reversible operation must register a concrete compensation, and documentation must identify irreversible boundaries.

## 6. Preserve the original cause

Cancellation and cleanup must not silently replace the failure that caused them. When both execution and rollback fail, both are retained.

## 7. Safe defaults

- `default(Option<T>)` is `None`.
- `default(Result<T, E>)` is invalid and fails loudly when consumed.
- branch failure cancels siblings by default;
- race losers are observed before return;
- rollback actions execute in reverse order.

## 8. Minimal dependencies

The runtime package should rely on the .NET base class library. Tooling, analyzers, and test frameworks belong outside the runtime dependency graph.

## 9. Preview APIs may change, semantics must be documented

Before 1.0, names and signatures can evolve. Every release must nevertheless state its concurrency, cancellation, exception, and rollback behavior precisely.

## 10. Performance follows correctness

Readonly value types and `ValueTask` reduce overhead in common paths, but performance changes must be benchmarked and must not weaken ownership or failure guarantees.
