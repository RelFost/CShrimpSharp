using CShrimpSharp;
using CShrimpSharp.Concurrency;
using CShrimpSharp.Transactions;

Result<int, Failure> quota = ParsePositiveInteger("21")
    .Map(static value => value * 2)
    .Tap(static value => Console.WriteLine($"Calculated quota: {value}"));

quota.Switch(
    value => Console.WriteLine($"Result succeeded: {value}"),
    failure => Console.WriteLine($"Result failed: {failure}"));

IReadOnlyList<string> loaded = await Shrimp.SyncAsync(
    LoadProfileAsync,
    LoadInventoryAsync,
    LoadMissionAsync);

Console.WriteLine($"Sync completed: {string.Join(", ", loaded)}");

RaceResult<string> race = await Shrimp.RaceAsync(
    WaitForPlayerAsync,
    WaitForTimeoutAsync);

Console.WriteLine($"Race winner #{race.WinnerIndex}: {race.Value}");

int credits = 100;
var inventory = new List<string>();

Result<Unit, Failure> purchase = await ShrimpTransaction.RunAsync<Unit>(
    async (transaction, cancellationToken) =>
    {
        const int price = 25;

        if (credits < price)
        {
            return Result.Failure<Unit>(
                new Failure("insufficient_credits", "Not enough credits."));
        }

        credits -= price;
        transaction.OnRollback(() => credits += price);

        inventory.Add("mobility-upgrade");
        transaction.OnRollback(() => inventory.Remove("mobility-upgrade"));

        await Task.Delay(10, cancellationToken);
        return Result.Success();
    });

Console.WriteLine(
    $"Purchase: {purchase}; credits: {credits}; inventory: {string.Join(", ", inventory)}");

static Result<int, Failure> ParsePositiveInteger(string value)
{
    if (!int.TryParse(value, out int parsed))
    {
        return Result.Failure<int>(
            new Failure("invalid_number", $"'{value}' is not an integer."));
    }

    return Result.Success(parsed).Ensure(
        static number => number > 0,
        static number => new Failure(
            "not_positive",
            $"The number must be positive, but was {number}."));
}

static async ValueTask<string> LoadProfileAsync(CancellationToken cancellationToken)
{
    await Task.Delay(25, cancellationToken);
    return "profile";
}

static async ValueTask<string> LoadInventoryAsync(CancellationToken cancellationToken)
{
    await Task.Delay(15, cancellationToken);
    return "inventory";
}

static async ValueTask<string> LoadMissionAsync(CancellationToken cancellationToken)
{
    await Task.Delay(5, cancellationToken);
    return "mission";
}

static async ValueTask<string> WaitForPlayerAsync(CancellationToken cancellationToken)
{
    await Task.Delay(20, cancellationToken);
    return "player-input";
}

static async ValueTask<string> WaitForTimeoutAsync(CancellationToken cancellationToken)
{
    await Task.Delay(200, cancellationToken);
    return "timeout";
}
