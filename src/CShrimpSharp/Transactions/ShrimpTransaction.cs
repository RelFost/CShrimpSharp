namespace CShrimpSharp.Transactions;

/// <summary>Implements an explicit local compensating transaction.</summary>
public sealed class ShrimpTransaction : IAsyncDisposable
{
    private readonly Stack<Func<CancellationToken, ValueTask>> _rollbackActions = [];

    /// <summary>Gets the current transaction state.</summary>
    public ShrimpTransactionState State { get; private set; } = ShrimpTransactionState.Active;

    /// <summary>Gets whether the transaction is no longer active.</summary>
    public bool IsCompleted => State is not ShrimpTransactionState.Active;

    /// <summary>Gets the number of registered compensations.</summary>
    public int CompensationCount => _rollbackActions.Count;

    /// <summary>Registers an asynchronous rollback action.</summary>
    public void OnRollback(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureActive();
        _rollbackActions.Push(action);
    }

    /// <summary>Registers a synchronous rollback action.</summary>
    public void OnRollback(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnRollback(_ =>
        {
            action();
            return ValueTask.CompletedTask;
        });
    }

    /// <summary>Executes an operation and registers its compensation after success.</summary>
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

    /// <summary>Executes a value-producing operation and registers a compensation based on its value.</summary>
    public async ValueTask<T> StepAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        Func<T, CancellationToken, ValueTask> rollback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(rollback);
        EnsureActive();

        T value = await operation(cancellationToken).ConfigureAwait(false);
        OnRollback(token => rollback(value, token));
        return value;
    }

    /// <summary>Commits this transaction and discards all compensations.</summary>
    public void Commit()
    {
        EnsureActive();
        _rollbackActions.Clear();
        State = ShrimpTransactionState.Committed;
    }

    /// <summary>Executes every compensation in reverse order.</summary>
    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (IsCompleted)
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

        State = errors is null
            ? ShrimpTransactionState.RolledBack
            : ShrimpTransactionState.RollbackFailed;

        if (errors is not null)
        {
            throw new AggregateException("One or more compensations failed.", errors);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!IsCompleted)
        {
            await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private void EnsureActive()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException($"Transaction is {State}.");
        }
    }
}
