namespace CShrimpSharp.Concurrency;

/// <summary>
/// Provides Verse-inspired structured-concurrency operations for .NET tasks.
/// </summary>
public static class Shrimp
{
    /// <summary>
    /// Starts all operations concurrently and returns their values in input order.
    /// A failing operation requests cancellation of its siblings.
    /// </summary>
    public static async ValueTask<IReadOnlyList<T>> SyncAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task<T>[] tasks = operationArray
            .Select(operation => RunSyncOperationAsync(operation, linkedCancellation))
            .ToArray();

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts all operations concurrently and returns their values in input order.
    /// </summary>
    public static ValueTask<IReadOnlyList<T>> SyncAsync<T>(
        params Func<CancellationToken, ValueTask<T>>[] operations) =>
        SyncAsync(operations, CancellationToken.None);

    /// <summary>
    /// Starts two differently typed operations concurrently and returns a typed tuple.
    /// </summary>
    public static async ValueTask<(T1 First, T2 Second)> SyncAsync<T1, T2>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task<T1> firstTask = RunSyncOperationAsync(first, linkedCancellation);
        Task<T2> secondTask = RunSyncOperationAsync(second, linkedCancellation);

        await Task.WhenAll(firstTask, secondTask).ConfigureAwait(false);

        return (
            await firstTask.ConfigureAwait(false),
            await secondTask.ConfigureAwait(false));
    }

    /// <summary>
    /// Returns the first completed operation and requests cancellation of all losers.
    /// Every child task is observed before the method returns.
    /// </summary>
    public static async ValueTask<RaceResult<T>> RaceAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        if (operationArray.Length == 0)
        {
            throw new ArgumentException(
                "At least one operation is required.",
                nameof(operations));
        }

        using var linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task<T>[] tasks = operationArray
            .Select(operation => operation(linkedCancellation.Token).AsTask())
            .ToArray();

        Task<T> winner = await Task.WhenAny(tasks).ConfigureAwait(false);
        int winnerIndex = Array.IndexOf(tasks, winner);

        TryCancelSilently(linkedCancellation);

        try
        {
            T value = await winner.ConfigureAwait(false);
            return new RaceResult<T>(winnerIndex, value);
        }
        finally
        {
            await ObserveAllAsync(tasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the first completed operation and requests cancellation of all losers.
    /// </summary>
    public static ValueTask<RaceResult<T>> RaceAsync<T>(
        params Func<CancellationToken, ValueTask<T>>[] operations) =>
        RaceAsync(operations, CancellationToken.None);

    /// <summary>
    /// Runs an operation with a timeout while preserving external cancellation.
    /// </summary>
    /// <exception cref="TimeoutException">The timeout elapsed before completion.</exception>
    public static async ValueTask<T> WithTimeoutAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (timeout <= TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        using var timeoutCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeoutCancellation.CancelAfter(timeout);

        try
        {
            return await operation(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested &&
                  timeoutCancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation exceeded {timeout}.");
        }
    }

    private static async Task<T> RunSyncOperationAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationTokenSource siblingCancellation)
    {
        try
        {
            return await operation(siblingCancellation.Token).ConfigureAwait(false);
        }
        catch
        {
            TryCancelSilently(siblingCancellation);
            throw;
        }
    }

    private static async ValueTask ObserveAllAsync(IEnumerable<Task> tasks)
    {
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch
        {
            // All child tasks are intentionally observed. The winner defines the race outcome.
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
            // Cancellation callbacks must not replace the original operation result.
        }
    }
}
