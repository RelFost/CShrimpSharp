# Краткий справочник API

Эта страница — компактная карта публичного API версии `0.3.0-preview.1`. Подробные гарантии и граничные случаи описаны в тематических руководствах.

## Базовые значения

| API | Назначение | Типичный сценарий |
| --- | --- | --- |
| `Option.Some(value)` | Создать присутствующее необязательное значение. | Поиск вернул ненулевое значение. |
| `Option.None<T>()` | Создать пустое необязательное значение. | Отсутствие ожидаемо и не требует ошибки. |
| `Option.From(value)` | Преобразовать nullable-ссылку в option. | Переменные окружения и необязательная конфигурация. |
| `Result.Success(value)` | Создать успешный результат с ошибкой `Failure`. | Начало явного success/failure-конвейера. |
| `Result<T,E>.Success(value)` | Создать успех с пользовательским типом ошибки. | Доменные ошибки. |
| `Result<T,E>.Failure(error)` | Создать неуспешный результат. | Ожидаемая ошибка проверки или бизнес-логики. |
| `Result.Try(action)` | Преобразовать обычное исключение в `Failure`. | Парсинг и границы системы. |
| `Validation<T,E>.Valid(value)` | Создать корректное значение. | Независимая проверка входных данных. |
| `Validation<T,E>.Invalid(errors)` | Накопить несколько ошибок. | Формы, конфигурация и пакетная проверка. |

## Композиция

```csharp
Result<int, Failure> result = Result
    .Try(() => int.Parse("42", CultureInfo.InvariantCulture))
    .Ensure(
        static value => value > 0,
        static value => new Failure("not_positive", $"Ожидалось положительное число, получено {value}."))
    .Map(static value => value * 2)
    .Tap(static value => Console.WriteLine($"Получено {value}"));
```

| Метод | Активная ветка | Результат |
| --- | --- | --- |
| `Map` | success / Some | Преобразованное значение. |
| `Bind` | success / Some | Результат следующей операции без вложенного контейнера. |
| `MapError` | failure | Ошибка другого типа. |
| `Ensure` | success | Исходный успех либо новая ошибка. |
| `Recover` | failure | Успешное резервное значение. |
| `RecoverWith` | failure | Замещающий результат. |
| `Tap` / `TapError` | соответствующая ветка | Исходный результат после побочного действия. |
| `GetValueOr` | любая | Значение либо готовый fallback. |
| `GetValueOrElse` | любая | Значение либо лениво вычисляемый fallback. |
| `ToOption` / `ToResult` | любая | Преобразование между option и result. |

Для асинхронного кода доступны `MapAsync`, `BindAsync` и `MatchAsync`.

## Структурированная конкурентность

```csharp
(string profile, int count, bool online) = await Shrimp.SyncAsync(
    LoadProfileAsync,
    LoadCountAsync,
    CheckOnlineAsync,
    cancellationToken);
```

| API | Условие завершения | Поведение при ошибке |
| --- | --- | --- |
| `SyncAsync` | Завершились все операции. | Ошибка запрашивает отмену соседей и пробрасывается. |
| `SyncSettledAsync` | Все операции перешли в конечное состояние. | Каждое исключение возвращается как `Result<T,Exception>`. |
| `RaceAsync` | Побеждает первое завершение. | Быстрая ошибка считается победителем и пробрасывается. |
| `RaceSuccessAsync` | Побеждает первый успех. | Если упали все операции, выбрасывается `AggregateException`. |
| `WithTimeoutAsync` | Операция успела до тайм-аута. | Тайм-аут даёт `TimeoutException`, внешняя отмена остаётся отменой. |

Typed-overload’ы `SyncAsync` поддерживают от двух до пяти операций с разными типами результата.

## Компенсирующие транзакции

```csharp
await using var transaction = new ShrimpTransaction();

string reservationId = await transaction.StepAsync(
    ReserveAsync,
    static (id, token) => ReleaseAsync(id, token),
    cancellationToken);

transaction.Commit();
```

| Член API | Значение |
| --- | --- |
| `OnRollback` | Зарегистрировать синхронную или асинхронную компенсацию. |
| `StepAsync` | Выполнить операцию и после успеха зарегистрировать откат. |
| `StepAsync<T>` | Передать результат операции в компенсацию. |
| `Commit` | Удалить компенсации и отметить транзакцию завершённой. |
| `RollbackAsync` | Выполнить компенсации в обратном порядке. |
| `State` | Получить `Active`, `Committed`, `RolledBack` или `RollbackFailed`. |

## Безопасные коллекции

```csharp
Option<string> second = values.AtOrNone(1);
Option<User> user = users.FindOrNone(userId);
Result<User, Failure> only = candidates.SingleResult();
```

Эти методы подходят, когда отсутствующий индекс, ключ или единственный элемент — нормальный доменный исход, а не ошибка программирования.
