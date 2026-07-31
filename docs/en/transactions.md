# Compensating transactions

`ShrimpTransaction` is a local saga-style helper. It is not a database transaction and does not provide distributed atomicity.

## Lifecycle

```text
Active -> Committed
Active -> RolledBack
Active -> RollbackFailed
```

The current state is exposed through `State`, and the number of registered compensations through `CompensationCount`.

## Returning values from steps

```csharp
await using var transaction = new ShrimpTransaction();

string fileId = await transaction.StepAsync(
    async token => await storage.UploadAsync(data, token),
    async (createdFileId, token) => await storage.DeleteAsync(createdFileId, token));

string recordId = await transaction.StepAsync(
    async token => await database.InsertAsync(fileId, token),
    async (createdRecordId, token) => await database.DeleteAsync(createdRecordId, token));

transaction.Commit();
```

If the second step fails, disposing the active transaction runs the first compensation. Compensations run in reverse registration order.

## Rollback failures

All compensations are attempted. Multiple failures are returned as `AggregateException`, and the state becomes `RollbackFailed`.

Keep rollback callbacks idempotent whenever possible.
