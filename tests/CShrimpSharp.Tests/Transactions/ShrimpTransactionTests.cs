using CShrimpSharp.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CShrimpSharp.Tests.Transactions;

[TestClass]
public sealed class ShrimpTransactionTests
{
    [TestMethod]
    public async Task FailedResult_RollsBackInReverseOrder()
    {
        var actions = new List<int>();

        Result<int, Failure> result = await ShrimpTransaction.RunAsync<int>(
            (transaction, _) =>
            {
                transaction.OnRollback(() => actions.Add(1));
                transaction.OnRollback(() => actions.Add(2));

                return ValueTask.FromResult(
                    Result.Failure<int>(new Failure("rejected", "The operation was rejected.")));
            });

        Assert.IsTrue(result.IsFailure);
        CollectionAssert.AreEqual(new[] { 2, 1 }, actions);
    }

    [TestMethod]
    public async Task SuccessfulResult_CommitsWithoutRollback()
    {
        bool rolledBack = false;

        Result<int, Failure> result = await ShrimpTransaction.RunAsync<int>(
            (transaction, _) =>
            {
                transaction.OnRollback(() => rolledBack = true);
                return ValueTask.FromResult(Result.Success(99));
            });

        Assert.AreEqual(99, result.Value);
        Assert.IsFalse(rolledBack);
    }

    [TestMethod]
    public async Task StepAsync_RegistersCompensationAfterSuccess()
    {
        var transaction = new ShrimpTransaction();
        int state = 0;

        await transaction.StepAsync(
            _ =>
            {
                state = 10;
                return ValueTask.CompletedTask;
            },
            _ =>
            {
                state = 0;
                return ValueTask.CompletedTask;
            });

        Assert.AreEqual(10, state);
        Assert.AreEqual(1, transaction.RollbackActionCount);

        await transaction.RollbackAsync();
        Assert.AreEqual(0, state);
    }

    [TestMethod]
    public async Task RollbackAsync_AttemptsEveryCompensationAndAggregatesFailures()
    {
        var transaction = new ShrimpTransaction();
        int attempted = 0;

        transaction.OnRollback(_ =>
        {
            attempted++;
            throw new InvalidOperationException("first rollback failure");
        });

        transaction.OnRollback(_ =>
        {
            attempted++;
            throw new ArgumentException("second rollback failure");
        });

        TransactionRollbackException? captured = null;

        try
        {
            await transaction.RollbackAsync();
        }
        catch (TransactionRollbackException exception)
        {
            captured = exception;
        }

        Assert.IsNotNull(captured);
        Assert.AreEqual(2, captured.RollbackExceptions.Count);
        Assert.AreEqual(2, attempted);
        Assert.AreEqual(TransactionState.Faulted, transaction.State);
    }

    [TestMethod]
    public async Task DisposeAsync_RollsBackActiveTransaction()
    {
        bool rolledBack = false;
        var transaction = new ShrimpTransaction();
        transaction.OnRollback(() => rolledBack = true);

        await transaction.DisposeAsync();

        Assert.IsTrue(rolledBack);
        Assert.AreEqual(TransactionState.RolledBack, transaction.State);
    }

    [TestMethod]
    public async Task RunAsync_FailedResultWithFailedRollback_ThrowsRollbackException()
    {
        TransactionRollbackException? captured = null;

        try
        {
            await ShrimpTransaction.RunAsync<int>((transaction, _) =>
            {
                transaction.OnRollback(_ =>
                    throw new InvalidOperationException("rollback failed"));

                return ValueTask.FromResult(
                    Result.Failure<int>(
                        new Failure("operation_rejected", "The operation was rejected.")));
            });
        }
        catch (TransactionRollbackException exception)
        {
            captured = exception;
        }

        Assert.IsNotNull(captured);
        Assert.AreEqual(1, captured.RollbackExceptions.Count);
        Assert.AreEqual("rollback failed", captured.RollbackExceptions[0].Message);
    }
}
