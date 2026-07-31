namespace CShrimpSharp.Transactions;

/// <summary>
/// Implements an explicit compensating transaction.
/// </summary>
/// <remarks>
/// This type does not snapshot arbitrary memory and is not a database transaction. Every reversible
/// change must register an explicit compensation action, which is executed in reverse order.
/// </remarks>
public sealed class ShrimpTransaction : IAsyncDisposable
{
    private readonly object _gate = new();
    private readonly Stack<Func<CancellationToken, ValueTask>> _rollbackActions = new();
    private TransactionState _state = TransactionState.Active;

    /// <summary>
    /// Gets the current transaction state.
    /// </summary>
    public TransactionState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets the number of registered compensation actions.
    /// </summary>
    public int RollbackActionCount
    {
        get
        {
            lock (_gate)
            {
                return _rollbackActions.Count;
            }
        }
    }

    /// <summary>
    /// Registers an asynchronous compensation action.
    /// </summary>
    public void OnRollback(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_gate)
        {
            EnsureActive();
            _rollbackActions.Push(action);
        }
    }

    /// <summary>
    /// Registers a parameterless asynchronous compensation action.
    /// </summary>
    public void OnRollback(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        OnRollback(_ => action());
    }

    /// <summary>
    /// Registers a synchronous compensation action.
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
    /// Executes one operation and registers its compensation only after the operation succeeds.
    /// </summary>
    public async ValueTask StepAsync(
        Func<CancellationToken, ValueTask> operation,
        Func<CancellationToken, ValueTask> rollback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(rollback);
        EnsureActiveThreadSafe();

        await operation(cancellationToken).ConfigureAwait(false);
        OnRollback(rollback);
    }

    /// <summary>
    /// Executes one value-producing operation and registers a compensation using its value.
    /// </summary>
    public async ValueTask<TValue> StepAsync<TValue>(
        Func<CancellationToken, ValueTask<TValue>> operation,
        Func<TValue, CancellationToken, ValueTask> rollback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(rollback);
        EnsureActiveThreadSafe();

        TValue value = await operation(cancellationToken).ConfigureAwait(false);
        OnRollback(token => rollback(value, token));
        return value;
    }

    /// <summary>
    /// Commits the transaction and discards all compensation actions.
    /// </summary>
    public void Commit()
    {
        lock (_gate)
        {
            EnsureActive();
            _rollbackActions.Clear();
            _state = TransactionState.Committed;
        }
    }

    /// <summary>
    /// Executes all compensation actions in reverse registration order.
    /// Every action is attempted even when an earlier compensation fails.
    /// </summary>
    public async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        Func<CancellationToken, ValueTask>[] rollbackActions;

        lock (_gate)
        {
            switch (_state)
            {
                case TransactionState.RolledBack:
                    return;
                case TransactionState.Committed:
                    throw new InvalidOperationException("A committed transaction cannot be rolled back.");
                case TransactionState.RollingBack:
                    throw new InvalidOperationException("The transaction is already rolling back.");
                case TransactionState.Faulted:
                    throw new InvalidOperationException("The transaction has already faulted during rollback.");
                case TransactionState.Active:
                    _state = TransactionState.RollingBack;
                    break;
                default:
                    throw new InvalidOperationException("The transaction is in an unknown state.");
            }

            rollbackActions = [.. _rollbackActions];
            _rollbackActions.Clear();
        }

        List<Exception>? exceptions = null;

        foreach (Func<CancellationToken, ValueTask> rollbackAction in rollbackActions)
        {
            try
            {
                await rollbackAction(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                exceptions ??= [];
                exceptions.Add(exception);
            }
        }

        lock (_gate)
        {
            _state = exceptions is null
                ? TransactionState.RolledBack
                : TransactionState.Faulted;
        }

        if (exceptions is not null)
        {
            throw new TransactionRollbackException(exceptions);
        }
    }

    /// <summary>
    /// Runs an operation inside a compensating transaction using <see cref="Failure" /> errors.
    /// A failed result triggers rollback; a successful result commits.
    /// </summary>
    public static ValueTask<Result<TValue, Failure>> RunAsync<TValue>(
        Func<ShrimpTransaction, CancellationToken, ValueTask<Result<TValue, Failure>>> operation,
        CancellationToken cancellationToken = default) =>
        RunAsync<TValue, Failure>(operation, cancellationToken);

    /// <summary>
    /// Runs an operation inside a compensating transaction.
    /// A failed result triggers rollback; a successful result commits.
    /// </summary>
    public static async ValueTask<Result<TValue, TError>> RunAsync<TValue, TError>(
        Func<ShrimpTransaction, CancellationToken, ValueTask<Result<TValue, TError>>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var transaction = new ShrimpTransaction();

        try
        {
            Result<TValue, TError> result =
                await operation(transaction, cancellationToken).ConfigureAwait(false);

            result.EnsureInitialized();

            if (result.IsSuccess)
            {
                transaction.Commit();
                return result;
            }

            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch (Exception executionException)
        {
            if (transaction.State is TransactionState.Active)
            {
                try
                {
                    await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (TransactionRollbackException rollbackException)
                {
                    var exceptions = new List<Exception>(
                        capacity: rollbackException.RollbackExceptions.Count + 1)
                    {
                        executionException,
                    };

                    exceptions.AddRange(rollbackException.RollbackExceptions);

                    throw new AggregateException(
                        "The transaction operation failed and one or more compensation actions also failed.",
                        exceptions);
                }
            }

            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (State is TransactionState.Active)
        {
            await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private void EnsureActiveThreadSafe()
    {
        lock (_gate)
        {
            EnsureActive();
        }
    }

    private void EnsureActive()
    {
        if (_state is not TransactionState.Active)
        {
            throw new InvalidOperationException(
                $"The transaction must be active, but its current state is {_state}.");
        }
    }
}
