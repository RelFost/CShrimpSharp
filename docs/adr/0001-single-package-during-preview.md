# ADR 0001: Ship one runtime package during preview

- Status: Accepted
- Date: 2026-07-31

## Context

Core values, concurrency helpers, and compensating transactions can be separated physically, but an early multi-package layout increases versioning, dependency, publishing, and discovery cost before the API boundaries are proven.

## Decision

The preview ships one runtime assembly and one NuGet package named `CShrimpSharp`.

Public namespaces are separated from the beginning:

- `CShrimpSharp`;
- `CShrimpSharp.Concurrency`;
- `CShrimpSharp.Transactions`.

Analyzers will be a separate package because they have a different runtime and deployment model.

## Consequences

- Installation and examples remain simple.
- Internal organization can evolve without coordinating multiple package releases.
- Consumers currently receive all runtime features.
- A later package split is possible without renaming public namespaces, but assembly identities and dependency graphs will still require migration notes.
