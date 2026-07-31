<div align="center">
  <img src="https://raw.githubusercontent.com/RelFost/CShrimpSharp/main/assets/cshrimp-logo.png" width="220" alt="CShrimpSharp logo: a shrimp inside the letter C">

# CShrimpSharp

**Verse-inspired failable operations, structured concurrency, and explicit compensating transactions for C# and .NET.**

[![CI](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C%23 14](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/github/license/RelFost/CShrimpSharp)](LICENSE)
[![Status: preview](https://img.shields.io/badge/status-preview-orange)](CHANGELOG.md)

[Русская документация](README.ru.md)
</div>

> [!IMPORTANT]
> CShrimpSharp is an independent, unofficial project. It is inspired by selected programming ideas exposed by Verse, but it is not a Verse compiler, runtime, compatibility layer, or Epic Games product.

## Why CShrimpSharp?

Ordinary exceptions, detached tasks, and manually coordinated rollback code make gameplay and service logic harder to reason about. CShrimpSharp provides a small set of explicit, composable primitives:

| Area | API | Purpose |
| --- | --- | --- |
| Failable operations | `Result<TValue, TError>`, `Failure` | Represent expected failure as data. |
| Optional values | `Option<TValue>` | Represent a present or absent non-null value. |
| Composition | `Map`, `Bind`, `Ensure`, `Traverse`, LINQ | Build pipelines without nested conditionals. |
| Structured concurrency | `Shrimp.SyncAsync`, `RaceAsync`, `ScopeAsync` | Tie child operations to a parent lifetime. |
| Child branches | `ShrimpScope.Branch` | Start work that must be joined or cancelled with its scope. |
| Compensation | `ShrimpTransaction` | Explicitly undo reversible changes in LIFO order. |

The preview deliberately favors understandable behavior over language-level magic.

## Requirements

- .NET 10 SDK
- C# 14

The library currently targets `net10.0`.

## Installation

After the first NuGet release:

```powershell
dotnet add package CShrimpSharp --prerelease
```

To build the repository directly:

```powershell
git clone https://github.com/RelFost/CShrimpSharp.git
cd CShrimpSharp
dotnet restore CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet run --project samples/CShrimpSharp.Example/CShrimpSharp.Example.csproj
```

## Quick start

### Failable operations

```csharp
using CShrimpSharp;

Result<int, Failure> quota = Result.Try(() => int.Parse("21"))
    .Ensure(
        static value => value > 0,
        static value => new Failure(
            "quota_not_positive",
            $"Quota must be positive, but was {value}."))
    .Map(static value => value * 2);

quota.Switch(
    value => Console.WriteLine($"Quota: {value}"),
    failure => Console.WriteLine(failure));
```

`Result<TValue, TError>` has an explicit uninitialized state. Accessing a default result throws instead of silently pretending that it is a success or failure.

### Optional values

```csharp
using CShrimpSharp;

Option<string> playerName = Option.From(
    Environment.GetEnvironmentVariable("PLAYER_NAME"));

string displayName = playerName
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Anonymous");
```

### `sync`-style concurrency

```csharp
using CShrimpSharp.Concurrency;

IReadOnlyList<string> loaded = await Shrimp.SyncAsync(
    async cancellationToken =>
    {
        await Task.Delay(30, cancellationToken);
        return "profile";
    },
    async cancellationToken =>
    {
        await Task.Delay(10, cancellationToken);
        return "inventory";
    });

Console.WriteLine(string.Join(", ", loaded));
```

All operations start together. Results preserve input order. When one operation fails, cancellation is requested for its siblings.

### `race`-style concurrency

```csharp
using CShrimpSharp.Concurrency;

RaceResult<string> winner = await Shrimp.RaceAsync(
    async cancellationToken =>
    {
        await Task.Delay(25, cancellationToken);
        return "player-input";
    },
    async cancellationToken =>
    {
        await Task.Delay(250, cancellationToken);
        return "timeout";
    });

Console.WriteLine($"Winner #{winner.WinnerIndex}: {winner.Value}");
```

The first completed operation determines the race outcome. Losing operations are cancelled and observed before `RaceAsync` returns, so cooperative children do not silently outlive the race.

### Structured branches

```csharp
using CShrimpSharp.Concurrency;

await Shrimp.ScopeAsync((scope, cancellationToken) =>
{
    scope.Branch(async token =>
    {
        await Task.Delay(20, token);
        Console.WriteLine("Telemetry flushed.");
    });

    scope.Branch(async token =>
    {
        await Task.Delay(10, token);
        Console.WriteLine("Checkpoint saved.");
    });

    return ValueTask.CompletedTask;
});
```

`ScopeAsync` seals the scope after the body returns, joins every registered branch, and cancels cooperative siblings when a branch fails.

### Explicit compensating transactions

```csharp
using CShrimpSharp;
using CShrimpSharp.Transactions;

int credits = 100;
var inventory = new List<string>();

Result<Unit, Failure> purchase = await ShrimpTransaction.RunAsync<Unit>(
    async (transaction, cancellationToken) =>
    {
        const int price = 25;

        if (credits < price)
        {
            return Result.Failure<Unit>(
                new Failure("insufficient_credits", "Not enough credits."));
        }

        credits -= price;
        transaction.OnRollback(() => credits += price);

        inventory.Add("mobility-upgrade");
        transaction.OnRollback(() => inventory.Remove("mobility-upgrade"));

        await Task.Delay(10, cancellationToken);
        return Result.Success();
    });
```

A successful result commits. A failed result or thrown exception runs registered compensations in reverse order.

> [!WARNING]
> `ShrimpTransaction` does not snapshot arbitrary memory and is not a database transaction. Every reversible change must register an explicit compensation. Compensation should be idempotent whenever practical.

## Verse concept mapping

| Verse idea | CShrimpSharp preview | Important difference |
| --- | --- | --- |
| Failable expression / failure context | `Result`, `Option`, `Bind`, `Ensure` | Implemented through ordinary C# values and methods. |
| `sync` | `Shrimp.SyncAsync` | Uses cooperative .NET cancellation and tasks. |
| `race` | `Shrimp.RaceAsync` | Losers must observe cancellation for prompt completion. |
| `branch` | `ShrimpScope.Branch` | Branch registration is explicit and scoped. |
| Transactional rollback | `ShrimpTransaction` | Rollback actions are explicit; arbitrary assignments are not reversible. |
| Effect checking | Planned Roslyn analyzers | C# does not gain Verse's compiler-enforced effect system. |

See [the detailed mapping](docs/verse-mapping.md), [architecture notes](docs/architecture.md), and the [documentation index](docs/README.md).

## Project layout

```text
CShrimpSharp/
├── src/CShrimpSharp/
│   ├── Core/
│   ├── Extensions/
│   ├── Concurrency/
│   └── Transactions/
├── tests/CShrimpSharp.Tests/
├── samples/CShrimpSharp.Example/
├── docs/
├── assets/
├── scripts/
├── .github/
├── README.ru.md
└── NOTICE.md
```

The preview ships as one NuGet package. Public namespaces are already separated so concurrency, transactions, and analyzers can become independent packages later without renaming the main API. See the [detailed repository structure](docs/repository-structure.md).

## Design boundaries

CShrimpSharp does **not**:

- parse or execute Verse source code;
- modify C# syntax;
- provide automatic rollback for arbitrary fields, properties, files, network calls, or database writes;
- force a task to stop when it ignores its `CancellationToken`;
- guarantee deterministic scheduling across machines;
- replace database transactions or distributed saga infrastructure.

## Development

```powershell
dotnet format CShrimpSharp.sln --verify-no-changes
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --output artifacts/packages
```

The helper scripts under `scripts/` run the same workflow on PowerShell and Bash.

## Roadmap

The first preview focuses on stable semantics and tests. Planned work includes richer async composition, timeout helpers, diagnostic events, Roslyn analyzers, transactional state wrappers, API compatibility baselines, and performance benchmarks. See the [roadmap](docs/roadmap.md).

## Contributing and security

Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. Report security-sensitive findings through GitHub's private security advisory flow as described in [SECURITY.md](SECURITY.md).

## License

CShrimpSharp is licensed under the [MIT License](LICENSE). See [NOTICE.md](NOTICE.md) for independence and trademark notices.
