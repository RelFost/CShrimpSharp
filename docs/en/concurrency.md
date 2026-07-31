# Concurrency

`SyncAsync` starts owned operations together and waits for all. Typed overloads return tuples. `RaceAsync` returns the first completion, cancels losers, and observes them before returning. `WithTimeoutAsync` uses cooperative cancellation and throws `TimeoutException` when the deadline expires.
