using System.Globalization;
using CShrimpSharp;
using CShrimpSharp.Collections;
using CShrimpSharp.Concurrency;
using CShrimpSharp.Transactions;

Result<int, Failure> parsed = Result
    .Try(() => int.Parse("21", CultureInfo.InvariantCulture))
    .Map(static value => value * 2)
    .Tap(static value => Console.WriteLine($"Parsed: {value}"));

string[] values = ["a", "b"];
Console.WriteLine(values.AtOrNone(1).GetValueOr("missing"));

(string profile, int count) = await Shrimp.SyncAsync(
    async token =>
    {
        await Task.Delay(10, token);
        return "profile";
    },
    async token =>
    {
        await Task.Delay(5, token);
        return 42;
    });

Console.WriteLine($"{profile}: {count}");

await using var transaction = new ShrimpTransaction();
string resource = await transaction.StepAsync(
    static _ => ValueTask.FromResult("resource-42"),
    static (id, _) =>
    {
        Console.WriteLine($"Rollback {id}");
        return ValueTask.CompletedTask;
    });
Console.WriteLine(resource);
transaction.Commit();
