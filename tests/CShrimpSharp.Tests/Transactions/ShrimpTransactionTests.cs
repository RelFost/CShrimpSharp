using CShrimpSharp.Transactions;

namespace CShrimpSharp.Tests.Transactions;

[TestClass]
public sealed class ShrimpTransactionTests
{
    [TestMethod]
    public async Task Rollback_IsLifo()
    {
        List<int> order = [];
        await using var transaction = new ShrimpTransaction();
        transaction.OnRollback(() => order.Add(1));
        transaction.OnRollback(() => order.Add(2));
        await transaction.RollbackAsync();
        CollectionAssert.AreEqual(new[] { 2, 1 }, order);
        Assert.AreEqual(ShrimpTransactionState.RolledBack, transaction.State);
    }

    [TestMethod]
    public async Task StepAsync_ReturnsValueAndRegistersCompensation()
    {
        int rolledBack = 0;
        await using var transaction = new ShrimpTransaction();
        int value = await transaction.StepAsync(static _ => ValueTask.FromResult(9), (created, _) => { rolledBack = created; return ValueTask.CompletedTask; });
        Assert.AreEqual(9, value);
        Assert.AreEqual(1, transaction.CompensationCount);
        await transaction.RollbackAsync();
        Assert.AreEqual(9, rolledBack);
    }

    [TestMethod]
    public async Task Commit_PreventsRollback()
    {
        bool called = false;
        await using var transaction = new ShrimpTransaction();
        transaction.OnRollback(() => called = true);
        transaction.Commit();
        await transaction.RollbackAsync();
        Assert.IsFalse(called);
        Assert.AreEqual(ShrimpTransactionState.Committed, transaction.State);
    }
}
