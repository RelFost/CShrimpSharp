# ADR 0002: Use explicit compensations instead of automatic state rollback

- Status: Accepted
- Date: 2026-07-31

## Context

Verse-inspired rollback is attractive, but a normal C# library cannot intercept every assignment, external call, collection mutation, file write, or database operation. Claiming automatic rollback would create false safety.

## Decision

`ShrimpTransaction` records caller-supplied compensation delegates. Compensations execute in reverse registration order. Every compensation is attempted, and failures are aggregated.

The transaction helper rolls back on a failed result or an exception and commits on success.

## Consequences

- Behavior is implementable and testable with ordinary C#.
- Callers must identify every reversible side effect.
- Irreversible actions remain irreversible.
- Compensation code can fail and must be designed carefully.
- The API resembles local saga compensation, not isolation or transactional memory.
