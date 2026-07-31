namespace CShrimpSharp.Concurrency;

/// <summary>
/// Verse-inspired structured-concurrency operations for .NET tasks.
/// </summary>
public static class Shrimp
{
    /// <summary>
    /// Runs a structured scope that waits for all registered branches before returning.
    /// </summary>
    public static async ValueTask ScopeAsync(
        Func<ShrimpScope, CancellationToken, ValueTask> body,
        CancellationToken cancellationToken = default,
        ShrimpScopeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(body);

        var scope = new ShrimpScope(cancellationToken, options);

        try
        {
            await body(scope, scope.Token).ConfigureAwait(false);
            await scope.JoinAsync().ConfigureAwait(false);
        }
        catch
        {
            await scope.DisposeSilentlyAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (!scope.IsDisposed)
            {
                await scope.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Runs a structured scope and returns the body's value after every branch completes.
    /// </summary>
    public static async ValueTask<TValue> ScopeAsync<TValue>(
        Func<ShrimpScope, CancellationToken, ValueTask<TValue>> body,
        CancellationToken cancellationToken = default,
        ShrimpScopeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(body);

        var scope = new ShrimpScope(cancellationToken, options);

        try
        {
            TValue value = await body(scope, scope.Token).ConfigureAwait(false);
            await scope.JoinAsync().ConfigureAwait(false);
            return value;
        }
        catch
        {
            await scope.DisposeSilentlyAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (!scope.IsDisposed)
            {
                await scope.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Runs all operations concurrently and completes after every operation succeeds.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static ValueTask SyncAsync(
        params Func<CancellationToken, ValueTask>[] operations) =>
        SyncAsync(operations, CancellationToken.None);

    /// <summary>
    /// Runs all operations concurrently and completes after every operation succeeds.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static ValueTask SyncAsync(
        CancellationToken cancellationToken,
        params Func<CancellationToken, ValueTask>[] operations) =>
        SyncAsync(operations, cancellationToken);

    /// <summary>
    /// Runs all operations concurrently and completes after every operation succeeds.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static async ValueTask SyncAsync(
        IEnumerable<Func<CancellationToken, ValueTask>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        Func<CancellationToken, ValueTask>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new Task[operationArray.Length];

        for (int index = 0; index < operationArray.Length; index++)
        {
            tasks[index] = RunSyncOperationAsync(
                operationArray[index],
                linkedCancellation);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs all operations concurrently and returns their values in input order.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static ValueTask<IReadOnlyList<TValue>> SyncAsync<TValue>(
        params Func<CancellationToken, ValueTask<TValue>>[] operations) =>
        SyncAsync(operations, CancellationToken.None);

    /// <summary>
    /// Runs all operations concurrently and returns their values in input order.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static ValueTask<IReadOnlyList<TValue>> SyncAsync<TValue>(
        CancellationToken cancellationToken,
        params Func<CancellationToken, ValueTask<TValue>>[] operations) =>
        SyncAsync(operations, cancellationToken);

    /// <summary>
    /// Runs all operations concurrently and returns their values in input order.
    /// A non-cancellation failure requests cancellation of sibling operations.
    /// </summary>
    public static async ValueTask<IReadOnlyList<TValue>> SyncAsync<TValue>(
        IEnumerable<Func<CancellationToken, ValueTask<TValue>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        Func<CancellationToken, ValueTask<TValue>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new Task<TValue>[operationArray.Length];

        for (int index = 0; index < operationArray.Length; index++)
        {
            tasks[index] = RunSyncOperationAsync(
                operationArray[index],
                linkedCancellation);
        }

        TValue[] values = await Task.WhenAll(tasks).ConfigureAwait(false);
        return values;
    }

    /// <summary>
    /// Returns the first operation to complete and cancels all losing operations.
    /// The method drains the losers before returning so no child operation outlives the race.
    /// </summary>
    public static ValueTask<RaceResult<TValue>> RaceAsync<TValue>(
        params Func<CancellationToken, ValueTask<TValue>>[] operations) =>
        RaceAsync(operations, CancellationToken.None);

    /// <summary>
    /// Returns the first operation to complete and cancels all losing operations.
    /// The method drains the losers before returning so no child operation outlives the race.
    /// </summary>
    public static ValueTask<RaceResult<TValue>> RaceAsync<TValue>(
        CancellationToken cancellationToken,
        params Func<CancellationToken, ValueTask<TValue>>[] operations) =>
        RaceAsync(operations, cancellationToken);

    /// <summary>
    /// Returns the first operation to complete and cancels all losing operations.
    /// The method drains the losers before returning so no child operation outlives the race.
    /// </summary>
    public static async ValueTask<RaceResult<TValue>> RaceAsync<TValue>(
        IEnumerable<Func<CancellationToken, ValueTask<TValue>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        Func<CancellationToken, ValueTask<TValue>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        if (operationArray.Length == 0)
        {
            throw new ArgumentException(
                "At least one operation is required for a race.",
                nameof(operations));
        }

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new Task<TValue>[operationArray.Length];

        for (int index = 0; index < operationArray.Length; index++)
        {
            tasks[index] = RunRaceOperationAsync(
                operationArray[index],
                linkedCancellation.Token);
        }

        Task<TValue> winner = await Task.WhenAny(tasks).ConfigureAwait(false);
        int winnerIndex = Array.IndexOf(tasks, winner);

        TryCancelSilently(linkedCancellation);

        try
        {
            TValue value = await winner.ConfigureAwait(false);
            return new RaceResult<TValue>(winnerIndex, value);
        }
        finally
        {
            await ObserveAllAsync(tasks).ConfigureAwait(false);
        }
    }

    private static async Task RunSyncOperationAsync(
        Func<CancellationToken, ValueTask> operation,
        CancellationTokenSource siblingCancellation)
    {
        try
        {
            await operation(siblingCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (siblingCancellation.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            TryCancelSilently(siblingCancellation);
            throw;
        }
    }

    private static async Task<TValue> RunSyncOperationAsync<TValue>(
        Func<CancellationToken, ValueTask<TValue>> operation,
        CancellationTokenSource siblingCancellation)
    {
        try
        {
            return await operation(siblingCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (siblingCancellation.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            TryCancelSilently(siblingCancellation);
            throw;
        }
    }

    private static async Task<TValue> RunRaceOperationAsync<TValue>(
        Func<CancellationToken, ValueTask<TValue>> operation,
        CancellationToken cancellationToken) =>
        await operation(cancellationToken).ConfigureAwait(false);

    private static async ValueTask ObserveAllAsync(IEnumerable<Task> tasks)
    {
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch
        {
            // Tasks are intentionally observed here. The winning task decides race outcome.
        }
    }

    private static void ValidateOperations<TDelegate>(TDelegate[] operations)
        where TDelegate : Delegate
    {
        for (int index = 0; index < operations.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(
                operations[index],
                $"operations[{index}]");
        }
    }

    private static void TryCancelSilently(CancellationTokenSource cancellation)
    {
        try
        {
            cancellation.Cancel();
        }
        catch (AggregateException)
        {
            // A cancellation callback must not replace the operation failure.
        }
    }
}
