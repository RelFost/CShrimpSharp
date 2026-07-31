<div align="center">
  <img src="https://raw.githubusercontent.com/RelFost/CShrimpSharp/main/assets/cshrimp-logo.png" width="220" alt="Логотип CShrimpSharp: креветка внутри буквы C">

# CShrimpSharp

**Вдохновлённые Verse операции с возможностью отказа, структурированная конкурентность и явные компенсирующие транзакции для C# и .NET.**

[English README](README.md)
</div>

> [!IMPORTANT]
> CShrimpSharp — независимый неофициальный проект. Он переносит отдельные идеи Verse в привычные конструкции C#, но не является компилятором Verse, средой выполнения Verse, слоем совместимости или продуктом Epic Games.

## Что входит в первую версию

| Область | API | Назначение |
| --- | --- | --- |
| Ожидаемые ошибки | `Result<TValue, TError>`, `Failure` | Ошибка как значение, а не как обычный путь через исключение. |
| Необязательные значения | `Option<TValue>` | Явное наличие или отсутствие ненулевого значения. |
| Композиция | `Map`, `Bind`, `Ensure`, `Traverse`, LINQ | Последовательные цепочки без вложенных проверок. |
| Структурированная конкурентность | `Shrimp.SyncAsync`, `RaceAsync`, `ScopeAsync` | Дочерние операции связаны со временем жизни родителя. |
| Ветви | `ShrimpScope.Branch` | Запущенная работа обязательно присоединяется к области или отменяется вместе с ней. |
| Компенсация изменений | `ShrimpTransaction` | Явный откат обратимых действий в порядке LIFO. |

## Требования

- .NET 10 SDK;
- C# 14.

Текущая целевая платформа библиотеки — `net10.0`.

## Сборка

```powershell
git clone https://github.com/RelFost/CShrimpSharp.git
cd CShrimpSharp
dotnet restore CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet run --project samples/CShrimpSharp.Example/CShrimpSharp.Example.csproj
```

После первой публикации в NuGet:

```powershell
dotnet add package CShrimpSharp --prerelease
```

## `Result` и `Option`

```csharp
using CShrimpSharp;

Result<int, Failure> quota = Result.Try(() => int.Parse("21"))
    .Ensure(
        static value => value > 0,
        static value => new Failure(
            "quota_not_positive",
            $"Квота должна быть положительной, получено: {value}."))
    .Map(static value => value * 2);

quota.Switch(
    value => Console.WriteLine($"Квота: {value}"),
    failure => Console.WriteLine(failure));

Option<string> playerName = Option.From(
    Environment.GetEnvironmentVariable("PLAYER_NAME"));

string displayName = playerName
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Anonymous");
```

У `Result<TValue, TError>` есть отдельное неинициализированное состояние. Чтение `default(Result<...>)` приводит к понятному исключению, а не маскируется под успех или ошибку.

## Аналог `sync`

```csharp
using CShrimpSharp.Concurrency;

IReadOnlyList<string> loaded = await Shrimp.SyncAsync(
    async cancellationToken =>
    {
        await Task.Delay(30, cancellationToken);
        return "profile";
    },
    async cancellationToken =>
    {
        await Task.Delay(10, cancellationToken);
        return "inventory";
    });
```

Операции стартуют вместе, а результаты возвращаются в исходном порядке. Ошибка одной операции запрашивает отмену остальных.

## Аналог `race`

```csharp
using CShrimpSharp.Concurrency;

RaceResult<string> winner = await Shrimp.RaceAsync(
    async cancellationToken =>
    {
        await Task.Delay(25, cancellationToken);
        return "player-input";
    },
    async cancellationToken =>
    {
        await Task.Delay(250, cancellationToken);
        return "timeout";
    });

Console.WriteLine($"Победитель #{winner.WinnerIndex}: {winner.Value}");
```

После завершения победителя проигравшие операции получают отмену и наблюдаются до возврата из `RaceAsync`. Быстрое завершение возможно только тогда, когда операции соблюдают переданный `CancellationToken`.

## Область с дочерними ветвями

```csharp
using CShrimpSharp.Concurrency;

await Shrimp.ScopeAsync((scope, cancellationToken) =>
{
    scope.Branch(async token =>
    {
        await Task.Delay(20, token);
        Console.WriteLine("Телеметрия отправлена.");
    });

    scope.Branch(async token =>
    {
        await Task.Delay(10, token);
        Console.WriteLine("Контрольная точка сохранена.");
    });

    return ValueTask.CompletedTask;
});
```

После завершения тела область закрывается для новых ветвей и ожидает уже зарегистрированные. Ошибка ветви по умолчанию запрашивает отмену соседних ветвей.

## Компенсирующая транзакция

```csharp
using CShrimpSharp;
using CShrimpSharp.Transactions;

int credits = 100;
var inventory = new List<string>();

Result<Unit, Failure> purchase = await ShrimpTransaction.RunAsync<Unit>(
    async (transaction, cancellationToken) =>
    {
        const int price = 25;

        if (credits < price)
        {
            return Result.Failure<Unit>(
                new Failure("insufficient_credits", "Недостаточно кредитов."));
        }

        credits -= price;
        transaction.OnRollback(() => credits += price);

        inventory.Add("mobility-upgrade");
        transaction.OnRollback(() => inventory.Remove("mobility-upgrade"));

        await Task.Delay(10, cancellationToken);
        return Result.Success();
    });
```

Успешный `Result` фиксирует транзакцию. Ошибка или исключение запускает зарегистрированные компенсации в обратном порядке.

> [!WARNING]
> `ShrimpTransaction` не делает снимок памяти и не заменяет транзакцию базы данных. Каждое обратимое изменение нужно явно снабдить компенсацией. По возможности компенсации должны быть идемпотентными.

## Соответствие идеям Verse

| Идея Verse | CShrimpSharp | Ограничение |
| --- | --- | --- |
| Failable expression / failure context | `Result`, `Option`, `Bind`, `Ensure` | Реализовано обычными типами и методами C#. |
| `sync` | `Shrimp.SyncAsync` | Использует задачи .NET и кооперативную отмену. |
| `race` | `Shrimp.RaceAsync` | Проигравшая операция должна реагировать на отмену. |
| `branch` | `ShrimpScope.Branch` | Ветвь явно регистрируется в области. |
| Откат транзакции | `ShrimpTransaction` | Откатываются только явно зарегистрированные действия. |
| Проверка эффектов | Будущие Roslyn-анализаторы | Обычная библиотека не добавляет систему эффектов в язык C#. |

Подробности находятся в [карте соответствия Verse](docs/verse-mapping.md), [описании архитектуры](docs/architecture.md) и [оглавлении документации](docs/README.md).

## Границы проекта

CShrimpSharp не выполняет Verse-код, не меняет синтаксис C#, не умеет автоматически откатывать произвольные присваивания, не способен принудительно остановить задачу, игнорирующую отмену, и не заменяет транзакции базы данных или полноценную распределённую saga-систему.

## Структура

```text
CShrimpSharp/
├── src/CShrimpSharp/
│   ├── Core/
│   ├── Extensions/
│   ├── Concurrency/
│   └── Transactions/
├── tests/CShrimpSharp.Tests/
├── samples/CShrimpSharp.Example/
├── docs/
├── assets/
├── scripts/
├── .github/
├── README.md
└── NOTICE.md
```

На стадии preview всё поставляется одним NuGet-пакетом. Пространства имён уже разделены, поэтому позже модули конкурентности, транзакций и анализаторов можно вынести в отдельные пакеты без переименования основного API. Подробное назначение каталогов и файлов описано в [docs/repository-structure.md](docs/repository-structure.md).

## Разработка

Полный локальный цикл:

```powershell
dotnet format CShrimpSharp.sln --verify-no-changes
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --output artifacts/packages
```

Готовые PowerShell- и Bash-сценарии находятся в `scripts/`. Рекомендуемые описание, темы и правила ветки GitHub перечислены в [docs/github-settings.md](docs/github-settings.md).

## Лицензия

Проект распространяется по лицензии [MIT](LICENSE). Информация о независимости проекта и товарных знаках находится в [NOTICE.md](NOTICE.md).
