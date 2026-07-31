# Changelog

## [Unreleased]

### Added

- `Result<TValue, TError>` equality, diagnostics, error mapping, recovery, tap, fallback, and option conversion helpers.
- Asynchronous `MapAsync`, `BindAsync`, and `MatchAsync` composition for result and option values.
- `SyncSettledAsync` and `RaceSuccessAsync` structured-concurrency operations.
- Typed `SyncAsync` overloads for two through five differently typed operations.
- Observable transaction lifecycle through `ShrimpTransactionState`.
- Value-producing `ShrimpTransaction.StepAsync<T>` operations.
- A broader behavioral test matrix running on .NET 8 and .NET 10.
- Public API analyzer scaffolding and baseline files.
- NuGet package consumer smoke tests in CI.
- Native AOT and trimming smoke tests in CI.
- Multi-target NuGet packaging for `net8.0` and `net10.0`.

### Changed

- Prepared package version `0.3.0-preview.1`.
- Updated PublicApiAnalyzers to version 5.6.0.

## [0.2.0-preview.1] - 2026-07-31

### Fixed

- Restored standard C# formatting and XML documentation for public preview APIs.

### Added

- Mirrored English and Russian documentation.
- Discord community links.
- `Validation<TValue, TError>`.
- Safe collection access helpers.
- Typed `SyncAsync` overloads and timeout helpers.
- GitHub Release and GitHub Packages publication workflow.

## [0.1.0-preview.1] - 2026-07-31

- Initial preview with `Result`, `Option`, structured concurrency, and compensating transactions.
