<div align="center">
  <img src="https://raw.githubusercontent.com/RelFost/CShrimpSharp/main/assets/cshrimp-logo.png" width="210" alt="CShrimpSharp logo: a shrimp shaped like the letter C">

# CShrimpSharp

**Verse-inspired explicit failure handling, structured concurrency, and compensating transactions for modern C# and .NET.**

[![CI](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml)
[![.NET 8 + 10](https://img.shields.io/badge/.NET-8%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)
[![C%23 14](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/github/license/RelFost/CShrimpSharp)](LICENSE)
[![Status: preview](https://img.shields.io/badge/status-preview-orange)](CHANGELOG.md)

[English](README.md) · [Русский](README.ru.md) · [Documentation](docs/en/README.md) · [Discord](https://discord.gg/QsKC34GbHS)

<sub>Current development line: <strong>0.3.0-preview.1</strong></sub>
</div>

> [!IMPORTANT]
> CShrimpSharp is an independent C# library inspired by selected Verse programming ideas. It is not a Verse compiler, runtime, compatibility layer, or Epic Games product.

## Why CShrimpSharp?

Expected failures, optional values, groups of child tasks, and reversible operations are common in games and services. CShrimpSharp provides explicit primitives for these cases without introducing a custom runtime or hidden scheduler.

| Area | Main API | Purpose |
| --- | --- | --- |
| Expected failures | `Result<TValue, TError>`, `Failure` | Represent recoverable failures as values. |
| Optional values | `Option<T>` | Represent a present or absent non-null value. |
| Validation | `Validation<TValue, TError>` | Accumulate multiple validation errors. |
| Composition | `Map`, `Bind`, `Ensure`, async extensions, LINQ | Build readable success/failure pipelines. |
| Safe collections | `AtOrNone`, `FindOrNone`, `SingleResult` | Avoid exceptions for ordinary lookup outcomes. |
| Structured concurrency | `SyncAsync`, `SyncSettledAsync`, `RaceAsync`, `RaceSuccessAsync` | Keep concurrent child operations tied to one parent operation. |
| Time limits | `WithTimeoutAsync` | Distinguish external cancellation from timeout. |
| Compensation | `ShrimpTransaction`, `StepAsync<T>` | Undo reversible changes in LIFO order. |

The preview favors predictable behavior and explicit contracts over language-level magic.

## Requirements

- .NET 8 or .NET 10 runtime;
- .NET 10 SDK for building the repository;
- C# 14 compiler.

The NuGet package targets both `net8.0` and `net10.0`.

## Installation

After publication to NuGet:

```powershell
dotnet add package CShrimpSharp --prerelease
```

To use the repository directly:

```powershell
git clone https://github.com/RelFost/CShrimpSharp.git
cd CShrimpSharp
dotnet restore CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test --solution CShrimpSharp.sln --configuration Release --no-build
```

## Quick start

### Expected failures

```csharp
using System.Globalization;
using CShrimpSharp;

Result<int, Failure> quota = Result
    .Try(() => int.Parse("21", CultureInfo.InvariantCulture))
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

`Result<TValue, TError>` has an explicit uninitialized state. Accessing a default result throws instead of silently treating it as success or failure.

### Optional values

```csharp
using CShrimpSharp;

Option<string> configuredName = Option.From(
    Environment.GetEnvironmentVariable("PLAYER_NAME"));

string displayName = configuredName
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Anonymous");
```

### Typed concurrent operations

```csharp
using CShrimpSharp.Concurrency;

(string profile, int itemCount, bool hasMail) = await Shrimp.SyncAsync(
    async token =>
    {
        await Task.Delay(30, token);
        return "profile-42";
    },
    async token =>
    {
        await Task.Delay(20, token);
        return 12;
    },
    async token =>
    {
        await Task.Delay(10, token);
        return true;
    });
```

All operations start together. Typed overloads return tuples for two to five differently typed operations.

### Collect every concurrent outcome

```csharp
using CShrimpSharp;
using CShrimpSharp.Concurrency;

IReadOnlyList<Result<string, Exception>> outcomes =
    await Shrimp.SyncSettledAsync(
    [
        async token =>
        {
            await Task.Delay(20, token);
            return "profile";
        },
        _ => ValueTask.FromException<string>(
            new InvalidOperationException("Inventory unavailable.")),
    ]);

foreach (Result<string, Exception> outcome in outcomes)
{
    Console.WriteLine(outcome);
}
```

Unlike `SyncAsync`, settled synchronization preserves every success and failure instead of failing the whole group at the first exception.

### First successful race

```csharp
using CShrimpSharp.Concurrency;

RaceResult<string> winner = await Shrimp.RaceSuccessAsync(
    _ => ValueTask.FromException<string>(new IOException("Primary failed.")),
    async token =>
    {
        await Task.Delay(25, token);
        return "secondary";
    });

Console.WriteLine($"Winner #{winner.WinnerIndex}: {winner.Value}");
```

`RaceAsync` selects the first completed operation. `RaceSuccessAsync` ignores failed contenders until one succeeds or all fail.

### Compensating transaction

```csharp
using CShrimpSharp.Transactions;

int credits = 100;
var inventory = new List<string>();

await using var transaction = new ShrimpTransaction();

int remainingCredits = await transaction.StepAsync(
    _ =>
    {
        credits -= 25;
        return ValueTask.FromResult(credits);
    },
    _ =>
    {
        credits += 25;
        return ValueTask.CompletedTask;
    });

await transaction.StepAsync(
    _ =>
    {
        inventory.Add("mobility-upgrade");
        return ValueTask.CompletedTask;
    },
    _ =>
    {
        inventory.Remove("mobility-upgrade");
        return ValueTask.CompletedTask;
    });

transaction.Commit();
Console.WriteLine($"Credits left: {remainingCredits}");
```

> [!WARNING]
> `ShrimpTransaction` does not snapshot arbitrary memory and is not a database transaction. Every reversible change must register an explicit compensation.

## Design guarantees

- `Option<T>` never intentionally stores `null` as a present value.
- `Result<TValue, TError>` distinguishes success, failure, and an invalid default state.
- `SyncAsync` preserves input order and requests sibling cancellation after failure.
- `SyncSettledAsync` observes and returns every child outcome.
- `RaceAsync` and `RaceSuccessAsync` observe losing tasks before returning.
- Timeouts remain distinguishable from caller-requested cancellation.
- Transaction compensations execute in reverse registration order.
- No operation can forcibly stop code that ignores its `CancellationToken`.

See [Architecture and guarantees](docs/en/architecture.md) for detailed contracts and edge cases.

## Documentation

| Guide | Description |
| --- | --- |
| [Getting started](docs/en/getting-started.md) | Installation, first project, and basic conventions. |
| [Core types](docs/en/core.md) | `Option`, `Result`, `Validation`, and composition. |
| [Asynchronous composition](docs/en/async-composition.md) | Async mapping, binding, recovery, and side effects. |
| [Structured concurrency](docs/en/concurrency.md) | Sync, settled sync, race, successful race, cancellation, and timeout. |
| [Compensating transactions](docs/en/transactions.md) | Transaction lifecycle, typed steps, rollback, and failures. |
| [Cookbook](docs/en/cookbook.md) | Practical recipes for application and game code. |
| [Architecture](docs/en/architecture.md) | Design boundaries and behavioral guarantees. |
| [Release process](docs/en/releasing.md) | Package validation, CI, NuGet, and GitHub Releases. |
| [Roadmap](docs/en/roadmap.md) | Planned work before the stable release. |

The complete index is available in [English](docs/en/README.md) and [Russian](docs/ru/README.md).

## Project layout

```text
CShrimpSharp/
├── src/CShrimpSharp/
│   ├── Core/
│   ├── Extensions/
│   ├── Collections/
│   ├── Concurrency/
│   └── Transactions/
├── tests/CShrimpSharp.Tests/
├── samples/CShrimpSharp.Example/
├── docs/en/
├── docs/ru/
├── assets/
├── scripts/
└── .github/
```

## Development

```powershell
dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln --configuration Release --no-restore
dotnet test --solution CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
```

CI additionally performs a local NuGet installation smoke test and trimming/Native AOT checks.

## Community

Questions, API proposals, and implementation discussions are welcome in **[CShrimp Discord](https://discord.gg/QsKC34GbHS)**.

## License

CShrimpSharp is available under the [MIT License](LICENSE). See [NOTICE.md](NOTICE.md) for attribution and project-status notes.
