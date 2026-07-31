<div align="center">
  <img src="https://raw.githubusercontent.com/RelFost/CShrimpSharp/main/assets/cshrimp-logo.png" width="210" alt="Логотип CShrimpSharp: креветка в форме буквы C">

# CShrimpSharp

**Вдохновлённые Verse явные ошибки, структурированная конкурентность и компенсирующие транзакции для современного C# и .NET.**

[![CI](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/RelFost/CShrimpSharp/actions/workflows/ci.yml)
[![.NET 8 + 10](https://img.shields.io/badge/.NET-8%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)
[![C%23 14](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/github/license/RelFost/CShrimpSharp)](LICENSE)
[![Status: preview](https://img.shields.io/badge/status-preview-orange)](CHANGELOG.md)

[English](README.md) · [Русский](README.ru.md) · [Документация](docs/ru/README.md) · [Discord](https://discord.gg/QsKC34GbHS)

<sub>Текущая линия разработки: <strong>0.3.0-preview.1</strong></sub>
</div>

> [!IMPORTANT]
> CShrimpSharp — независимая библиотека для C#, вдохновлённая отдельными идеями Verse. Она не является компилятором Verse, средой выполнения, слоем совместимости или продуктом Epic Games.

## Зачем нужен CShrimpSharp?

Ожидаемые ошибки, необязательные значения, группы дочерних задач и обратимые действия постоянно встречаются в играх и сервисах. CShrimpSharp предоставляет для них явные примитивы без отдельной среды выполнения и скрытого планировщика.

| Область | Основной API | Назначение |
| --- | --- | --- |
| Ожидаемые ошибки | `Result<TValue, TError>`, `Failure` | Представление исправимых ошибок как значений. |
| Необязательные значения | `Option<T>` | Явное наличие или отсутствие ненулевого значения. |
| Валидация | `Validation<TValue, TError>` | Накопление нескольких ошибок проверки. |
| Композиция | `Map`, `Bind`, `Ensure`, async-расширения, LINQ | Читаемые цепочки успеха и ошибки. |
| Безопасные коллекции | `AtOrNone`, `FindOrNone`, `SingleResult` | Обычные промахи поиска без исключений. |
| Структурированная конкурентность | `SyncAsync`, `SyncSettledAsync`, `RaceAsync`, `RaceSuccessAsync` | Связывает дочерние операции со временем жизни родителя. |
| Ограничение времени | `WithTimeoutAsync` | Отличает внешний запрос отмены от тайм-аута. |
| Компенсация | `ShrimpTransaction`, `StepAsync<T>` | Откат обратимых изменений в порядке LIFO. |

Preview-версия ставит предсказуемость и явные контракты выше языковой магии.

## Требования

- среда выполнения .NET 8 или .NET 10;
- .NET 10 SDK для сборки репозитория;
- компилятор C# 14.

NuGet-пакет ориентирован одновременно на `net8.0` и `net10.0`.

## Установка

После публикации в NuGet:

```powershell
dotnet add package CShrimpSharp --prerelease
```

Для работы непосредственно с репозиторием:

```powershell
git clone https://github.com/RelFost/CShrimpSharp.git
cd CShrimpSharp
dotnet restore CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test --solution CShrimpSharp.sln --configuration Release --no-build
```

## Быстрый старт

### Ожидаемые ошибки

```csharp
using System.Globalization;
using CShrimpSharp;

Result<int, Failure> quota = Result
    .Try(() => int.Parse("21", CultureInfo.InvariantCulture))
    .Ensure(
        static value => value > 0,
        static value => new Failure(
            "quota_not_positive",
            $"Квота должна быть положительной, получено: {value}."))
    .Map(static value => value * 2);

quota.Switch(
    value => Console.WriteLine($"Квота: {value}"),
    failure => Console.WriteLine(failure));
```

У `Result<TValue, TError>` есть отдельное неинициализированное состояние. Обращение к значению `default` приводит к понятному исключению, а не маскируется под успех или ошибку.

### Необязательные значения

```csharp
using CShrimpSharp;

Option<string> configuredName = Option.From(
    Environment.GetEnvironmentVariable("PLAYER_NAME"));

string displayName = configuredName
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Anonymous");
```

### Типизированные конкурентные операции

```csharp
using CShrimpSharp.Concurrency;

(string profile, int itemCount, bool hasMail) = await Shrimp.SyncAsync(
    async token =>
    {
        await Task.Delay(30, token);
        return "profile-42";
    },
    async token =>
    {
        await Task.Delay(20, token);
        return 12;
    },
    async token =>
    {
        await Task.Delay(10, token);
        return true;
    });
```

Все операции запускаются вместе. Типизированные перегрузки возвращают кортежи для двух–пяти операций с разными типами результатов.

### Сбор всех конкурентных исходов

```csharp
using CShrimpSharp;
using CShrimpSharp.Concurrency;

IReadOnlyList<Result<string, Exception>> outcomes =
    await Shrimp.SyncSettledAsync(
    [
        async token =>
        {
            await Task.Delay(20, token);
            return "profile";
        },
        _ => ValueTask.FromException<string>(
            new InvalidOperationException("Inventory unavailable.")),
    ]);

foreach (Result<string, Exception> outcome in outcomes)
{
    Console.WriteLine(outcome);
}
```

В отличие от `SyncAsync`, settled-вариант сохраняет каждый успех и каждую ошибку, а не завершает всю группу первой ошибкой.

### Гонка до первого успеха

```csharp
using CShrimpSharp.Concurrency;

RaceResult<string> winner = await Shrimp.RaceSuccessAsync(
    _ => ValueTask.FromException<string>(new IOException("Primary failed.")),
    async token =>
    {
        await Task.Delay(25, token);
        return "secondary";
    });

Console.WriteLine($"Победитель #{winner.WinnerIndex}: {winner.Value}");
```

`RaceAsync` выбирает первую завершившуюся операцию. `RaceSuccessAsync` пропускает неудачные варианты, пока один не завершится успешно или не завершатся ошибкой все.

### Компенсирующая транзакция

```csharp
using CShrimpSharp.Transactions;

int credits = 100;
var inventory = new List<string>();

await using var transaction = new ShrimpTransaction();

int remainingCredits = await transaction.StepAsync(
    _ =>
    {
        credits -= 25;
        return ValueTask.FromResult(credits);
    },
    _ =>
    {
        credits += 25;
        return ValueTask.CompletedTask;
    });

await transaction.StepAsync(
    _ =>
    {
        inventory.Add("mobility-upgrade");
        return ValueTask.CompletedTask;
    },
    _ =>
    {
        inventory.Remove("mobility-upgrade");
        return ValueTask.CompletedTask;
    });

transaction.Commit();
Console.WriteLine($"Осталось кредитов: {remainingCredits}");
```

> [!WARNING]
> `ShrimpTransaction` не создаёт снимок произвольной памяти и не является транзакцией базы данных. Для каждого обратимого изменения нужно явно зарегистрировать компенсацию.

## Гарантии поведения

- `Option<T>` намеренно не хранит `null` как присутствующее значение.
- `Result<TValue, TError>` различает успех, ошибку и недопустимое состояние `default`.
- `SyncAsync` сохраняет входной порядок и запрашивает отмену соседних операций после ошибки.
- `SyncSettledAsync` наблюдает и возвращает все исходы дочерних операций.
- `RaceAsync` и `RaceSuccessAsync` наблюдают проигравшие задачи до возврата.
- Тайм-аут можно отличить от отмены, запрошенной вызывающим кодом.
- Компенсации транзакции выполняются в обратном порядке регистрации.
- Библиотека не способна принудительно остановить код, игнорирующий `CancellationToken`.

Подробные контракты и крайние случаи описаны в разделе [Архитектура и гарантии](docs/ru/architecture.md).

## Документация

| Руководство | Содержание |
| --- | --- |
| [Начало работы](docs/ru/getting-started.md) | Установка, первый проект и основные соглашения. |
| [Основные типы](docs/ru/core.md) | `Option`, `Result`, `Validation` и композиция. |
| [Асинхронная композиция](docs/ru/async-composition.md) | Асинхронные преобразования, связывание, восстановление и побочные действия. |
| [Структурированная конкурентность](docs/ru/concurrency.md) | Sync, settled sync, race, гонка до успеха, отмена и тайм-аут. |
| [Компенсирующие транзакции](docs/ru/transactions.md) | Жизненный цикл, типизированные шаги, откат и ошибки. |
| [Практические рецепты](docs/ru/cookbook.md) | Прикладные сценарии для игр и сервисов. |
| [Архитектура](docs/ru/architecture.md) | Границы проекта и гарантии поведения. |
| [Выпуск версии](docs/ru/releasing.md) | Package validation, CI, NuGet и GitHub Releases. |
| [План развития](docs/ru/roadmap.md) | Работа, запланированная до стабильной версии. |

Полное оглавление доступно на [русском](docs/ru/README.md) и [английском](docs/en/README.md).

## Структура проекта

```text
CShrimpSharp/
├── src/CShrimpSharp/
│   ├── Core/
│   ├── Extensions/
│   ├── Collections/
│   ├── Concurrency/
│   └── Transactions/
├── tests/CShrimpSharp.Tests/
├── samples/CShrimpSharp.Example/
├── docs/en/
├── docs/ru/
├── assets/
├── scripts/
└── .github/
```

## Разработка

```powershell
dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln --configuration Release --no-restore
dotnet test --solution CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
```

CI дополнительно устанавливает собранный NuGet-пакет в чистый проект и проверяет trimming/Native AOT.

## Сообщество

Вопросы, предложения по API и обсуждение реализации приветствуются в **[Discord CShrimp](https://discord.gg/QsKC34GbHS)**.

## Лицензия

CShrimpSharp распространяется по [лицензии MIT](LICENSE). Статус проекта и дополнительные сведения находятся в [NOTICE.md](NOTICE.md).
