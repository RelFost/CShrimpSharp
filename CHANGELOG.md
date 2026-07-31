# Changelog

All notable changes to CShrimpSharp are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project intends to follow [Semantic Versioning](https://semver.org/) after the public API reaches a stable release.

## [Unreleased]

### Planned

- Roslyn analyzer package.
- Additional timeout and cancellation helpers.
- Benchmarks and API compatibility baselines.

## [0.1.0-preview.1] - 2026-07-31

### Added

- `Result<TValue, TError>`, `Failure`, `Unit`, and `Option<TValue>`.
- Synchronous, asynchronous, collection, and LINQ composition extensions.
- `Shrimp.SyncAsync`, `Shrimp.RaceAsync`, and structured `Shrimp.ScopeAsync` branches.
- Explicit LIFO compensation through `ShrimpTransaction`.
- MSTest coverage for core, concurrency, and transaction behavior.
- Example application, bilingual README files, architecture documentation, CI, and packaging workflow.

[Unreleased]: https://github.com/RelFost/CShrimpSharp/compare/v0.1.0-preview.1...HEAD
[0.1.0-preview.1]: https://github.com/RelFost/CShrimpSharp/releases/tag/v0.1.0-preview.1
