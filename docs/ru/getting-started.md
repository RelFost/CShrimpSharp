# Начало работы

## Установка

```powershell
dotnet add package CShrimpSharp --version 0.3.0-preview.1
```

## Разбор значения без управления логикой через исключения

```csharp
using System.Globalization;
using CShrimpSharp;

Result<int, Failure> port = Result
    .Try(() => int.Parse("8080", CultureInfo.InvariantCulture))
    .Ensure(
        static value => value is > 0 and <= 65535,
        static value => new Failure("invalid_port", $"Порт {value} вне допустимого диапазона."));

port.Switch(
    value => Console.WriteLine($"Слушаем порт {value}"),
    error => Console.WriteLine($"Ошибка конфигурации: {error}"));
```

## Безопасный доступ к коллекции

```csharp
using CShrimpSharp;
using CShrimpSharp.Collections;

string[] names = ["Ada", "Grace"];
string selected = names.AtOrNone(1).GetValueOr("anonymous");
```

## Параллельная загрузка

```csharp
using CShrimpSharp.Concurrency;

(string profile, int notifications) = await Shrimp.SyncAsync(
    async token =>
    {
        await Task.Delay(20, token);
        return "profile-42";
    },
    async token =>
    {
        await Task.Delay(10, token);
        return 3;
    });
```

## Транзакция с компенсацией

```csharp
using CShrimpSharp.Transactions;

await using var transaction = new ShrimpTransaction();

string reservationId = await transaction.StepAsync(
    async token =>
    {
        await Task.Delay(10, token);
        return "reservation-42";
    },
    async (id, token) =>
    {
        await Task.Delay(10, token);
        Console.WriteLine($"Отменено: {id}");
    });

transaction.Commit();
```
