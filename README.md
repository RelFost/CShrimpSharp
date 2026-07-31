<div align="center">

# CShrimpSharp

**Verse-inspired explicit failure handling, structured concurrency, and compensating transactions for modern C# and .NET.**

[Русский](README.ru.md) · [Documentation](docs/en/README.md) · [Discord](https://discord.gg/QsKC34GbHS)

[![CI](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/RelFost/CShrimpSharp)](LICENSE)
[![Status: preview](https://img.shields.io/badge/status-preview-orange)](CHANGELOG.md)

</div>

> [!IMPORTANT]
> CShrimpSharp is a C# library, not a Verse compiler, runtime, compatibility layer, or Epic Games product.

## Features

- `Option<T>` for explicit optional values.
- `Result<TValue, TError>` for expected failures as data.
- `Validation<TValue, TError>` for accumulating multiple validation errors.
- `Map`, `Bind`, `Ensure`, LINQ, `Sequence`, and `Traverse` composition.
- Safe list and dictionary access without ordinary control-flow exceptions.
- `SyncAsync`, typed sync overloads, `RaceAsync`, timeout helpers, and owned scopes.
- Explicit LIFO compensating transactions.

## Requirements

- .NET 10 SDK
- C# 14

## Installation

After publication to NuGet:

```powershell
dotnet add package CShrimpSharp --prerelease
```

## Quick example

```csharp
using CShrimpSharp;

Result<int, Failure> result = Result.Try(() => int.Parse("21"))
    .Ensure(
        static value => value > 0,
        static value => new Failure("not_positive", $"Expected a positive value, got {value}."))
    .Map(static value => value * 2);

result.Switch(
    value => Console.WriteLine(value),
    error => Console.WriteLine(error));
```

## Build

```powershell
dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln --configuration Release --no-restore
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
```

## Documentation

- [English documentation](docs/en/README.md)
- [Русская документация](docs/ru/README.md)
- [Release and package publication](docs/en/releasing.md)
- [Roadmap](docs/en/roadmap.md)

## Community

Questions and API discussions: **[CShrimp Discord](https://discord.gg/QsKC34GbHS)**.

## License

[MIT](LICENSE). See [NOTICE.md](NOTICE.md).
