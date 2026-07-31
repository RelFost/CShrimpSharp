# Getting started

## Install

```powershell
dotnet add package CShrimpSharp --version 0.3.0-preview.1
```

## Parse without exception-driven control flow

```csharp
using System.Globalization;
using CShrimpSharp;

Result<int, Failure> port = Result
    .Try(() => int.Parse("8080", CultureInfo.InvariantCulture))
    .Ensure(
        static value => value is > 0 and <= 65535,
        static value => new Failure("invalid_port", $"Port {value} is outside the valid range."));

port.Switch(
    value => Console.WriteLine($"Listening on {value}"),
    error => Console.WriteLine($"Configuration error: {error}"));
```

## Optional lookup

```csharp
using CShrimpSharp;
using CShrimpSharp.Collections;

string[] names = ["Ada", "Grace"];
string selected = names.AtOrNone(1).GetValueOr("anonymous");
```

## Concurrent loading

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

## Transaction with compensation

```csharp
using CShrimpSharp.Transactions;

await using var transaction = new ShrimpTransaction();

string reservationId = await transaction.StepAsync(
    reserve: async token =>
    {
        await Task.Delay(10, token);
        return "reservation-42";
    },
    rollback: async (id, token) =>
    {
        await Task.Delay(10, token);
        Console.WriteLine($"Cancelled {id}");
    });

transaction.Commit();
```
