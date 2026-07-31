# Roadmap

The roadmap describes direction, not a compatibility promise. Preview priorities may change after real-world use.

## 0.1 preview — foundation

- `Result`, `Option`, `Failure`, and `Unit`.
- Synchronous, asynchronous, collection, and LINQ composition.
- `SyncAsync`, `RaceAsync`, `ScopeAsync`, and `Branch`.
- Explicit compensating transactions.
- Tests, example application, CI, NuGet packaging, and design documentation.

## 0.2 preview — async ergonomics

- Timeout and deadline helpers built on cancellation.
- Async `Sequence` and `Traverse` variants.
- More complete exception aggregation tests.
- Diagnostic hooks for scope, branch, race, and rollback lifecycle events.
- Cancellation behavior tests for already-cancelled tokens and synchronous delegate failures.

## 0.3 preview — analyzers

- New `CShrimpSharp.Analyzers` project and NuGet package.
- Initial diagnostics for unsafe result access and ignored cancellation.
- Code fixes where behavior can be changed without guessing intent.
- Analyzer documentation with false-positive and suppression policy.

## 0.4 preview — state and integration

- Transaction-aware state wrappers for explicitly managed values.
- Adapters for common hosting lifetimes.
- Integration guidance for ASP.NET Core, game servers, Unity, and other .NET hosts.
- Benchmarks for result composition, scopes, races, and compensation registration.

## 0.9 release candidate

- Public API review and naming freeze.
- API compatibility baseline.
- Trimming and Native AOT validation where applicable.
- Expanded package documentation and migration notes.
- Performance regression thresholds.

## 1.0

- Stable documented semantics.
- Semantic versioning for the public API.
- Supported-target policy and release cadence.
- Analyzer package remains optional.

## Deliberately out of scope

- A Verse parser, compiler, VM, or interpreter.
- Automatic reversal of arbitrary C# assignments.
- Forced termination of tasks or threads.
- A distributed transaction coordinator.
- Deterministic scheduling of arbitrary .NET code.
