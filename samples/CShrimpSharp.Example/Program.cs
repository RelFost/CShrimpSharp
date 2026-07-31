using System.Globalization;
using CShrimpSharp;
using CShrimpSharp.Collections;
using CShrimpSharp.Concurrency;

Result<int, Failure> parsed = Result
    .Try(() => int.Parse("21", CultureInfo.InvariantCulture))
    .Map(value => value * 2);

parsed.Switch(Console.WriteLine, Console.WriteLine);

string[] values = ["a", "b"];

Console.WriteLine(
    values
        .AtOrNone(1)
        .GetValueOr("missing"));

(string profile, int count) = await Shrimp.SyncAsync(
    async cancellationToken =>
    {
        await Task.Delay(10, cancellationToken);
        return "profile";
    },
    async cancellationToken =>
    {
        await Task.Delay(5, cancellationToken);
        return 42;
    });

Console.WriteLine($"{profile}: {count}");
