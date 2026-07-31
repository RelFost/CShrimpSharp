using CShrimpSharp.Concurrency;
using CShrimpSharp.Transactions;

namespace CShrimpSharp.Tests;

[TestClass]
public sealed class Release03Tests
{
    [TestMethod]
    public void Result_Equality_UsesActiveBranch()
    {
        Result<int, string> first = Result<int, string>.Success(42);
        Result<int, string> second = Result<int, string>.Success(42);
        Result<int, string> failure = Result<int, string>.Failure("error");

        Assert.AreEqual(first, second);
        Assert.IsTrue(first == second);
        Assert.AreNotEqual(first, failure);
        Assert.AreEqual("Success(42)", first.ToString());
    }

    [TestMethod]
    public void Result_Default_IsRejected()
    {
        Result<int, string> result = default;
        Assert.ThrowsExactly<InvalidOperationException>(() => result.ToString());
    }

    [TestMethod]
    public void Result_Combinators_TransformAndRecover()
    {
        Result<int, string> success = Result<int, string>.Success(2)
            .Map(static value => value * 3)
            .Ensure(static value => value > 0, static _ => "invalid")
            .MapError(static error => error.ToUpperInvariant());

        Result<int, string> recovered = Result<int, string>.Failure("missing")
            .Recover(static _ => 7);

        Assert.AreEqual(6, success.Value);
        Assert.AreEqual(7, recovered.Value);
    }

    [TestMethod]
    public async Task Result_AsyncCombinators_PreserveBranches()
    {
        Result<int, string> result = await Result<int, string>.Success(4)
            .MapAsync(static value => ValueTask.FromResult(value * 2));

        Result<int, string> failure = await Result<int, string>.Failure("error")
            .BindAsync(static value => ValueTask.FromResult(Result<int, string>.Success(value + 1)));

        Assert.AreEqual(8, result.Value);
        Assert.AreEqual("error", failure.Error);
    }

    [TestMethod]
    public async Task Option_AsyncCombinators_HandleSomeAndNone()
    {
        Option<int> some = await Option.Some(3)
            .MapAsync(static value => ValueTask.FromResult(value + 2));
        Option<int> none = await Option.None<int>()
            .BindAsync(static value => ValueTask.FromResult(Option.Some(value + 1)));

        Assert.AreEqual(5, some.Value);
        Assert.IsTrue(none.IsNone);
    }

    [TestMethod]
    public async Task SyncSettledAsync_PreservesOrderAndFailures()
    {
        IReadOnlyList<Result<int, Exception>> results = await Shrimp.SyncSettledAsync<int>(
            static _ => ValueTask.FromResult(1),
            static _ => ValueTask.FromException<int>(new InvalidOperationException("boom")),
            static _ => ValueTask.FromResult(3));

        Assert.AreEqual(3, results.Count);
        Assert.AreEqual(1, results[0].Value);
        Assert.IsInstanceOfType<InvalidOperationException>(results[1].Error);
        Assert.AreEqual(3, results[2].Value);
    }

    [TestMethod]
    public async Task RaceSuccessAsync_SkipsFailures()
    {
        RaceResult<int> result = await Shrimp.RaceSuccessAsync<int>(
            static _ => ValueTask.FromException<int>(new InvalidOperationException()),
            async cancellationToken =>
            {
                await Task.Delay(20, cancellationToken);
                return 42;
            });

        Assert.AreEqual(1, result.WinnerIndex);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task RaceSuccessAsync_AggregatesWhenAllFail()
    {
        await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
            await Shrimp.RaceSuccessAsync<int>(
                static _ => ValueTask.FromException<int>(new InvalidOperationException()),
                static _ => ValueTask.FromException<int>(new ArgumentException())));
    }

    [TestMethod]
    public async Task TypedSyncAsync_ReturnsFiveValuesInOrder()
    {
        (int first, string second, bool third, double fourth, long fifth) = await Shrimp.SyncAsync(
            static _ => ValueTask.FromResult(1),
            static _ => ValueTask.FromResult("two"),
            static _ => ValueTask.FromResult(true),
            static _ => ValueTask.FromResult(4d),
            static _ => ValueTask.FromResult(5L));

        Assert.AreEqual((1, "two", true, 4d, 5L), (first, second, third, fourth, fifth));
    }

    [TestMethod]
    public async Task Transaction_TypedStep_RegistersValueCompensation()
    {
        var rolledBack = 0;
        await using var transaction = new ShrimpTransaction();

        int value = await transaction.StepAsync(
            static _ => ValueTask.FromResult(42),
            (created, _) =>
            {
                rolledBack = created;
                return ValueTask.CompletedTask;
            });

        Assert.AreEqual(42, value);
        Assert.AreEqual(1, transaction.CompensationCount);
        await transaction.RollbackAsync();
        Assert.AreEqual(42, rolledBack);
        Assert.AreEqual(ShrimpTransactionState.RolledBack, transaction.State);
    }

    [TestMethod]
    public async Task Transaction_Rollback_IsLifo()
    {
        List<int> order = [];
        await using var transaction = new ShrimpTransaction();
        transaction.OnRollback(() => order.Add(1));
        transaction.OnRollback(() => order.Add(2));

        await transaction.RollbackAsync();
        CollectionAssert.AreEqual(new[] { 2, 1 }, order);
    }

    [TestMethod]
    public async Task Transaction_Rollback_AttemptsEveryCompensation()
    {
        var completed = false;
        await using var transaction = new ShrimpTransaction();
        transaction.OnRollback(() => completed = true);
        transaction.OnRollback(static () => throw new InvalidOperationException("boom"));

        await Assert.ThrowsExactlyAsync<AggregateException>(async () => await transaction.RollbackAsync());
        Assert.IsTrue(completed);
        Assert.AreEqual(ShrimpTransactionState.RollbackFailed, transaction.State);
    }
}
