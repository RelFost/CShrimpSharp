namespace CShrimpSharp.Transactions;

/// <summary>
/// Implements an explicit local compensating transaction.
/// Registered rollback actions are executed in reverse order.
/// </summary>
public sealed class ShrimpTransaction : IAsyncDisposable
{
    private readonly Stack<Func<CancellationToken, ValueTask>> _rollbackActions = [];
    private bool _completed;

    /// <summary>
    /// Registers an asynchronous rollback action.
    /// </summary>
    public void OnRollback(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureActive();
        _rollbackActions.Push(action);
    }

    /// <summary>
    /// Registers a synchronous rollback action.
    /// </summary>
    public void OnRollback(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        OnRollback(_ =>
        {
            action();
            return ValueTask.CompletedTask;
        });
    }

    /// <summary>
    /// Executes an operation and registers its compensation only after success.
    /// </summary>
    public async ValueTask StepAsync(
        Func<CancellationToken, ValueTask> operation,
        Func<CancellationToken, ValueTask> rollback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(rollback);
        EnsureActive();

        await operation(cancellationToken).ConfigureAwait(false);
        OnRollback(rollback);
    }

    /// <summary>
    /// Commits this transaction and discards all registered compensations.
    /// </summary>
    public void Commit()
    {
        EnsureActive();
        _rollbackActions.Clear();
        _completed = true;
    }

    /// <summary>
    /// Executes every registered compensation in reverse order.
    /// All compensations are attempted even when one fails.
    /// </summary>
    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            return;
        }

        List<Exception>? errors = null;

        while (_rollbackActions.TryPop(out Func<CancellationToken, ValueTask>? action))
        {
            try
            {
                await action(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                errors ??= [];
                errors.Add(exception);
            }
        }

        _completed = true;

        if (errors is not null)
        {
            throw new AggregateException("One or more compensations failed.", errors);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private void EnsureActive()
    {
        if (_completed)
        {
            throw new InvalidOperationException("Transaction is completed.");
        }
    }
}
