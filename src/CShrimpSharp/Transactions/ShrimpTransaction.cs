namespace CShrimpSharp.Transactions;

/// <summary>Represents an explicit local compensating transaction with LIFO rollback.</summary>
/// <remarks>
/// The transaction does not snapshot memory or integrate with a database transaction manager.
/// Every reversible operation must register an explicit compensation.
/// </remarks>
/// <example>
/// <code>
/// await using var transaction = new ShrimpTransaction();
/// await transaction.StepAsync(ReserveAsync, ReleaseAsync, cancellationToken);
/// transaction.Commit();
/// </code>
/// </example>
public sealed class ShrimpTransaction : IAsyncDisposable
{
    private readonly Stack<Func<CancellationToken, ValueTask>> _rollbackActions = [];

    /// <summary>Gets the current transaction lifecycle state.</summary>
    public ShrimpTransactionState State { get; private set; } = ShrimpTransactionState.Active;
    /// <summary>Gets whether the transaction can no longer accept operations or compensations.</summary>
    public bool IsCompleted => State != ShrimpTransactionState.Active;
    /// <summary>Gets the number of currently registered compensation actions.</summary>
    public int CompensationCount => _rollbackActions.Count;

    /// <summary>Registers an asynchronous compensation to execute during rollback.</summary>
    public void OnRollback(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureActive();
        _rollbackActions.Push(action);
    }

    /// <summary>Registers a synchronous compensation to execute during rollback.</summary>
    public void OnRollback(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnRollback(_ => { action(); return ValueTask.CompletedTask; });
    }

    /// <summary>Executes an operation and registers its compensation only after successful completion.</summary>
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

    /// <summary>Executes an operation, registers a compensation that receives its result, and returns that result.</summary>
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

    /// <summary>Commits the transaction and discards all registered compensations.</summary>
    public void Commit()
    {
        EnsureActive();
        _rollbackActions.Clear();
        State = ShrimpTransactionState.Committed;
    }

    /// <summary>Executes all registered compensations in reverse registration order.</summary>
    /// <exception cref="AggregateException">One or more compensations failed; all compensations were still attempted.</exception>
    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (IsCompleted) return;
        List<Exception>? errors = null;
        while (_rollbackActions.TryPop(out Func<CancellationToken, ValueTask>? action))
        {
            try { await action(cancellationToken).ConfigureAwait(false); }
            catch (Exception exception) { (errors ??= []).Add(exception); }
        }

        State = errors is null ? ShrimpTransactionState.RolledBack : ShrimpTransactionState.RollbackFailed;
        if (errors is not null) throw new AggregateException("One or more compensations failed.", errors);
    }

    /// <summary>Rolls back an active transaction; committed or already rolled-back transactions are left unchanged.</summary>
    public async ValueTask DisposeAsync()
    {
        if (!IsCompleted) await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private void EnsureActive()
    {
        if (State != ShrimpTransactionState.Active)
            throw new InvalidOperationException($"Transaction is not active. Current state: {State}.");
    }
}
