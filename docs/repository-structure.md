# Repository structure

The repository starts with one runtime package and keeps each behavior in a clearly named area. This avoids premature package fragmentation while preserving a path to separate packages later.

```text
CShrimpSharp/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   └── workflows/
├── .vscode/
├── assets/
├── docs/
│   └── adr/
├── samples/
│   └── CShrimpSharp.Example/
├── scripts/
├── src/
│   └── CShrimpSharp/
│       ├── Concurrency/
│       ├── Core/
│       ├── Exceptions/
│       ├── Extensions/
│       └── Transactions/
├── tests/
│   └── CShrimpSharp.Tests/
│       ├── Concurrency/
│       ├── Core/
│       └── Transactions/
├── CHANGELOG.md
├── CONTRIBUTING.md
├── CShrimpSharp.sln
├── Directory.Build.props
├── global.json
├── LICENSE
├── NOTICE.md
├── NuGet.Config
├── README.md
└── README.ru.md
```

## Runtime source

### `Core/`

Contains the values that define expected success, failure, and absence:

- `Result<TValue, TError>` — explicit success or failure with an invalid default state;
- `Option<TValue>` — present non-null value or `None`;
- `Failure` — stable code, readable message, and optional source exception;
- `Unit` — successful completion without a meaningful payload.

These types remain in the root `CShrimpSharp` namespace because they are the common vocabulary for all future modules.

### `Extensions/`

Contains composition that can be added without increasing the size of the core structs:

- `ResultExtensions.cs` — `Map`, `Bind`, `Ensure`, recovery, LINQ, and conversion;
- `ResultAsyncExtensions.cs` — asynchronous `Map`, `Bind`, and `Tap`;
- `OptionExtensions.cs` — optional-value composition and LINQ;
- `EnumerableResultExtensions.cs` — `Sequence` and `Traverse`.

Keep an extension in the narrowest file whose name explains its behavior. A new family of operations should receive its own file rather than turning `ResultExtensions.cs` into a catch-all.

### `Concurrency/`

Contains owned asynchronous work:

- `Shrimp` — `SyncAsync`, `RaceAsync`, and `ScopeAsync`;
- `ShrimpScope` — explicit child branches that are joined or cancelled with their owner;
- `ShrimpScopeOptions` — stable configuration for scope policy;
- `RaceResult<TValue>` — winner index and winner value.

Public delegates receive a `CancellationToken`. APIs accept factories rather than already-running tasks so the library owns creation, cancellation, observation, and completion.

### `Transactions/`

Contains explicit local compensation:

- `ShrimpTransaction` — compensation registration, reversible steps, commit, and LIFO rollback;
- `TransactionState` — lifecycle state;
- `TransactionRollbackException` — all rollback failures after every compensation was attempted.

This directory must not introduce claims of isolation or automatic memory rollback. Database and distributed transaction integrations should live in separate packages.

### `Exceptions/`

Contains exceptions that describe invalid API state rather than expected domain failure. Expected business outcomes belong in `Result`.

## Tests

Test directories mirror runtime areas. Each public semantic rule should have a focused test, especially:

- default and null behavior;
- ordering and first-failure behavior;
- cancellation propagation and child lifetime;
- rollback order and rollback-failure aggregation;
- repeated lifecycle calls and invalid state transitions.

## Samples

The example project is deliberately small and executable in CI. README snippets should remain consistent with it, but the sample should demonstrate complete flows rather than duplicate every API.

## Documentation and ADRs

General contracts live directly under `docs/`. A decision that constrains future API design receives an ADR under `docs/adr/`. ADRs are append-only historical records: supersede an old decision with a new ADR instead of silently rewriting the original rationale.

## Future physical split

A later split can use the existing namespace boundaries:

```text
CShrimpSharp                 Core values and composition
CShrimpSharp.Concurrency     Structured concurrency
CShrimpSharp.Transactions    Compensation primitives
CShrimpSharp.Analyzers       Optional Roslyn diagnostics
```

Do not split until independent versioning, dependencies, or adoption patterns justify the additional packages.
