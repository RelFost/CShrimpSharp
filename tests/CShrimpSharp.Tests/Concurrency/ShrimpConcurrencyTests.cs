using CShrimpSharp.Concurrency;

namespace CShrimpSharp.Tests.Concurrency;

[TestClass]
public sealed class ShrimpConcurrencyTests
{
    [TestMethod]
    public async Task TypedSync_ReturnsFiveValues()
    {
        var result = await Shrimp.SyncAsync(
            static _ => ValueTask.FromResult(1), static _ => ValueTask.FromResult("2"),
            static _ => ValueTask.FromResult(3d), static _ => ValueTask.FromResult(true),
            static _ => ValueTask.FromResult('5'));
        Assert.AreEqual((1, "2", 3d, true, '5'), result);
    }

    [TestMethod]
    public async Task SyncSettled_PreservesSuccessAndFailure()
    {
        IReadOnlyList<Result<int, Exception>> results = await Shrimp.SyncSettledAsync<int>([
            static _ => ValueTask.FromResult(1),
            static _ => ValueTask.FromException<int>(new InvalidOperationException("boom")),
        ]);
        Assert.IsTrue(results[0].IsSuccess);
        Assert.IsTrue(results[1].IsFailure);
    }

    [TestMethod]
    public async Task RaceSuccess_SkipsEarlyFailure()
    {
        RaceResult<int> result = await Shrimp.RaceSuccessAsync<int>([
            static _ => ValueTask.FromException<int>(new InvalidOperationException()),
            static _ => ValueTask.FromResult(7),
        ]);
        Assert.AreEqual(1, result.WinnerIndex);
        Assert.AreEqual(7, result.Value);
    }

    [TestMethod]
    public async Task Timeout_ThrowsTimeoutException() =>
        await Assert.ThrowsExactlyAsync<TimeoutException>(async () =>
            await Shrimp.WithTimeoutAsync<int>(async token => { await Task.Delay(500, token); return 1; }, TimeSpan.FromMilliseconds(10)));
}
