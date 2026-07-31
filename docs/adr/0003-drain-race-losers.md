# ADR 0003: Observe race losers before returning

- Status: Accepted
- Date: 2026-07-31

## Context

Returning immediately after a race winner completes can leave losing tasks running. Those tasks may retain resources, mutate state after the parent has moved on, or fail without observation.

## Decision

`Shrimp.RaceAsync` requests cancellation and observes all losing tasks before returning or propagating the winner's outcome.

## Consequences

- Child failures are observed.
- Cooperative children do not outlive the race.
- A loser that ignores cancellation can delay the race indefinitely.
- The API prioritizes structured lifetime over minimum winner latency.
