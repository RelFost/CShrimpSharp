# Компенсирующие транзакции

`ShrimpTransaction` — локальный saga-помощник. Это не транзакция базы данных и не гарантия распределённой атомарности.

## Жизненный цикл

```text
Active -> Committed
Active -> RolledBack
Active -> RollbackFailed
```

Текущее состояние доступно через `State`, число зарегистрированных компенсаций — через `CompensationCount`.

## Шаги с возвращаемым значением

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

Если второй шаг упадёт, `DisposeAsync` активной транзакции выполнит первую компенсацию. Компенсации выполняются в обратном порядке.

## Ошибки rollback

Выполняются все компенсации. Несколько ошибок возвращаются через `AggregateException`, состояние меняется на `RollbackFailed`.

Компенсации желательно делать идемпотентными.
