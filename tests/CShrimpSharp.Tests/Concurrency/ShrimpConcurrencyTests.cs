using CShrimpSharp.Concurrency;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Concurrency;

[TestClass]
public sealed class ShrimpConcurrencyTests
{
    [TestMethod]
    public async Task SyncAsync_ReturnsValuesInInputOrder()
    {
        IReadOnlyList<int> values = await Shrimp.SyncAsync(FirstAsync, SecondAsync);

        CollectionAssert.AreEqual(new[] { 1, 2 }, values.ToArray());

        static async ValueTask<int> FirstAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(30, cancellationToken);
            return 1;
        }

        static async ValueTask<int> SecondAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(5, cancellationToken);
            return 2;
        }
    }

    [TestMethod]
    public async Task RaceAsync_CancelsAndDrainsLoser()
    {
        var loserCancelled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        RaceResult<int> result = await Shrimp.RaceAsync(WinnerAsync, LoserAsync);

        Assert.AreEqual(0, result.WinnerIndex);
        Assert.AreEqual(42, result.Value);
        Assert.IsTrue(await loserCancelled.Task.WaitAsync(TimeSpan.FromSeconds(1)));

        static async ValueTask<int> WinnerAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            return 42;
        }

        async ValueTask<int> LoserAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return -1;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                loserCancelled.TrySetResult(true);
                throw;
            }
        }
    }

    [TestMethod]
    public async Task ScopeAsync_WaitsForRegisteredBranches()
    {
        int completedBranches = 0;

        await Shrimp.ScopeAsync((scope, _) =>
        {
            scope.Branch(async cancellationToken =>
            {
                await Task.Delay(10, cancellationToken);
                Interlocked.Increment(ref completedBranches);
            });

            return ValueTask.CompletedTask;
        });

        Assert.AreEqual(1, completedBranches);
    }

    [TestMethod]
    public async Task ScopeAsync_BranchFailureCancelsSibling()
    {
        var siblingCancelled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        bool failed = false;

        try
        {
            await Shrimp.ScopeAsync((scope, _) =>
            {
                scope.Branch(async cancellationToken =>
                {
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        siblingCancelled.TrySetResult(true);
                        throw;
                    }
                });

                scope.Branch(static async _ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("branch failed");
                });

                return ValueTask.CompletedTask;
            });
        }
        catch (InvalidOperationException exception) when (exception.Message == "branch failed")
        {
            failed = true;
        }

        Assert.IsTrue(failed);
        Assert.IsTrue(await siblingCancelled.Task.WaitAsync(TimeSpan.FromSeconds(1)));
    }

    [TestMethod]
    public async Task RaceAsync_RejectsEmptyOperationSet()
    {
        bool thrown = false;

        try
        {
            await Shrimp.RaceAsync<int>(
                Array.Empty<Func<CancellationToken, ValueTask<int>>>());
        }
        catch (ArgumentException)
        {
            thrown = true;
        }

        Assert.IsTrue(thrown);
    }

    [TestMethod]
    public async Task DisposeAsync_ConcurrentCallersWaitForTheSameCleanup()
    {
        var branchStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseBranch = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var scope = new ShrimpScope();

        scope.Branch(async _ =>
        {
            branchStarted.SetResult(true);
            await releaseBranch.Task;
        });

        await branchStarted.Task;

        Task firstDispose = scope.DisposeAsync().AsTask();
        Task secondDispose = scope.DisposeAsync().AsTask();

        Assert.IsFalse(firstDispose.IsCompleted);
        Assert.IsFalse(secondDispose.IsCompleted);

        releaseBranch.SetResult(true);
        await Task.WhenAll(firstDispose, secondDispose);
    }
}
