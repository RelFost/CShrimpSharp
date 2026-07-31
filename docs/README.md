# Documentation

This directory documents the contracts and boundaries of CShrimpSharp. Runtime behavior should not depend on undocumented assumptions.

## Start here

- [Architecture](architecture.md) — package boundaries, ownership, cancellation, failure, and rollback semantics.
- [Design principles](design-principles.md) — rules used when accepting or rejecting API designs.
- [Verse concept mapping](verse-mapping.md) — what is inspired by Verse and what cannot be reproduced by a C# library.
- [Repository structure](repository-structure.md) — purpose of the main directories and files.
- [Roadmap](roadmap.md) — preview milestones and possible future packages.
- [Release process](releasing.md) — package validation and manual NuGet publication.
- [Recommended GitHub settings](github-settings.md) — description, topics, branch rules, and repository features.

## Architecture decisions

- [ADR-0001: one package during preview](adr/0001-single-package-during-preview.md)
- [ADR-0002: explicit compensating transactions](adr/0002-explicit-compensating-transactions.md)
- [ADR-0003: drain race losers](adr/0003-drain-race-losers.md)

When a public behavior changes, update the relevant document, tests, XML comments, and `CHANGELOG.md` in the same pull request.
