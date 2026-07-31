# Transactions

`ShrimpTransaction` is an explicit compensation stack, not a database transaction. Register compensations immediately after a reversible operation or use `StepAsync`. Rollback executes newest compensation first.
