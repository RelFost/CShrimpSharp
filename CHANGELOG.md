# Changelog

## [Unreleased]

## [0.3.0-preview.1] - 2026-07-31

### Added

- Equality, diagnostics, recovery, error mapping, tapping, and fallback combinators for `Result`.
- Asynchronous `MapAsync`, `BindAsync`, and `MatchAsync` composition.
- `SyncSettledAsync` and `RaceSuccessAsync`.
- Typed `SyncAsync` overloads for two through five differently typed operations.
- Transaction lifecycle state, compensation count, and `StepAsync<T>`.
- Test matrix for core, concurrency, and transactions on .NET 8 and .NET 10.
- Public API analyzer scaffolding.
- NuGet package, trimming, and Native AOT smoke checks.
- Expanded English and Russian documentation with practical examples.
- Restored branded README presentation with the project logo, compatibility badges, API tables, and richer navigation.
- Added the project icon to NuGet package metadata.

### Changed

- Package now targets `net8.0` and `net10.0`.

## [0.2.0-preview.1] - 2026-07-31

- Validation, safe collections, typed synchronization, timeout helpers, bilingual documentation, and release workflows.

## [0.1.0-preview.1] - 2026-07-31

- Initial preview with `Result`, `Option`, structured concurrency, and compensating transactions.
