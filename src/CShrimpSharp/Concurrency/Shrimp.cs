namespace CShrimpSharp.Concurrency;

/// <summary>Provides structured-concurrency helpers for related .NET operations.</summary>
/// <remarks>
/// Child operations are started by the caller and remain tied to the lifetime of the parent method.
/// Losing or sibling tasks are observed before race operations return.
/// </remarks>
/// <example>
/// <code>
/// (string profile, int count) = await Shrimp.SyncAsync(
///     LoadProfileAsync,
///     LoadCountAsync,
///     cancellationToken);
/// </code>
/// </example>
public static class Shrimp
{
    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static async ValueTask<IReadOnlyList<T>> SyncAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] items = [.. operations];
        ValidateOperations(items);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks = items.Select(item => RunSyncOperationAsync(item, linked)).ToArray();
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static ValueTask<IReadOnlyList<T>> SyncAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        SyncAsync(operations, CancellationToken.None);

    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static async ValueTask<(T1 First, T2 Second)> SyncAsync<T1, T2>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        CancellationToken cancellationToken = default)
    {
        Task<T1> a = first(cancellationToken).AsTask();
        Task<T2> b = second(cancellationToken).AsTask();
        await Task.WhenAll(a, b).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false));
    }

    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static async ValueTask<(T1 First, T2 Second, T3 Third)> SyncAsync<T1, T2, T3>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        CancellationToken cancellationToken = default)
    {
        Task<T1> a = first(cancellationToken).AsTask();
        Task<T2> b = second(cancellationToken).AsTask();
        Task<T3> c = third(cancellationToken).AsTask();
        await Task.WhenAll(a, b, c).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false));
    }

    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static async ValueTask<(T1 First, T2 Second, T3 Third, T4 Fourth)> SyncAsync<T1, T2, T3, T4>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        Func<CancellationToken, ValueTask<T4>> fourth,
        CancellationToken cancellationToken = default)
    {
        Task<T1> a = first(cancellationToken).AsTask();
        Task<T2> b = second(cancellationToken).AsTask();
        Task<T3> c = third(cancellationToken).AsTask();
        Task<T4> d = fourth(cancellationToken).AsTask();
        await Task.WhenAll(a, b, c, d).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false), await d.ConfigureAwait(false));
    }

    /// <summary>Starts the supplied operations concurrently and returns their values in input order.</summary>
    /// <remarks>If an operation fails, sibling cancellation is requested.</remarks>
    public static async ValueTask<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth)> SyncAsync<T1, T2, T3, T4, T5>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        Func<CancellationToken, ValueTask<T4>> fourth,
        Func<CancellationToken, ValueTask<T5>> fifth,
        CancellationToken cancellationToken = default)
    {
        Task<T1> a = first(cancellationToken).AsTask();
        Task<T2> b = second(cancellationToken).AsTask();
        Task<T3> c = third(cancellationToken).AsTask();
        Task<T4> d = fourth(cancellationToken).AsTask();
        Task<T5> e = fifth(cancellationToken).AsTask();
        await Task.WhenAll(a, b, c, d, e).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false), await d.ConfigureAwait(false), await e.ConfigureAwait(false));
    }

    /// <summary>Runs all operations concurrently and returns every success or failure in input order.</summary>
    /// <remarks>External cancellation is preserved as cancellation rather than converted into a result.</remarks>
    public static async ValueTask<IReadOnlyList<Result<T, Exception>>> SyncSettledAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] items = [.. operations];
        ValidateOperations(items);
        Task<Result<T, Exception>>[] tasks = items.Select(async item =>
        {
            try
            {
                return Result<T, Exception>.Success(await item(cancellationToken).ConfigureAwait(false));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return Result<T, Exception>.Failure(exception);
            }
        }).ToArray();
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>Returns the first completed operation and cancels the remaining contenders.</summary>
    public static async ValueTask<RaceResult<T>> RaceAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] items = [.. operations];
        ValidateOperations(items);
        if (items.Length == 0) throw new ArgumentException("At least one operation is required.", nameof(operations));

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks = items.Select(item => item(linked.Token).AsTask()).ToArray();
        Task<T> winner = await Task.WhenAny(tasks).ConfigureAwait(false);
        int winnerIndex = Array.IndexOf(tasks, winner);
        TryCancelSilently(linked);
        try
        {
            return new RaceResult<T>(winnerIndex, await winner.ConfigureAwait(false));
        }
        finally
        {
            await ObserveAllAsync(tasks).ConfigureAwait(false);
        }
    }

    /// <summary>Returns the first completed operation and cancels the remaining contenders.</summary>
    public static ValueTask<RaceResult<T>> RaceAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        RaceAsync(operations, CancellationToken.None);

    /// <summary>Returns the first successful operation and cancels the remaining contenders.</summary>
    /// <exception cref="AggregateException">Every operation failed.</exception>
    public static async ValueTask<RaceResult<T>> RaceSuccessAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] items = [.. operations];
        ValidateOperations(items);
        if (items.Length == 0) throw new ArgumentException("At least one operation is required.", nameof(operations));

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var remaining = items.Select((item, index) => (Index: index, Task: item(linked.Token).AsTask())).ToList();
        List<Exception> failures = [];

        while (remaining.Count > 0)
        {
            Task<T> completed = await Task.WhenAny(remaining.Select(item => item.Task)).ConfigureAwait(false);
            int position = remaining.FindIndex(item => ReferenceEquals(item.Task, completed));
            (int index, Task<T> task) = remaining[position];
            remaining.RemoveAt(position);

            try
            {
                T value = await task.ConfigureAwait(false);
                TryCancelSilently(linked);
                await ObserveAllAsync(remaining.Select(item => item.Task)).ConfigureAwait(false);
                return new RaceResult<T>(index, value);
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                failures.Add(exception);
            }
        }

        throw new AggregateException("Every race operation failed.", failures);
    }

    /// <summary>Returns the first successful operation and cancels the remaining contenders.</summary>
    /// <exception cref="AggregateException">Every operation failed.</exception>
    public static ValueTask<RaceResult<T>> RaceSuccessAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        RaceSuccessAsync(operations, CancellationToken.None);

    /// <summary>Runs an operation with a timeout while preserving caller-requested cancellation.</summary>
    /// <exception cref="TimeoutException">The timeout elapsed before the operation completed.</exception>
    public static async ValueTask<T> WithTimeoutAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (timeout <= TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan) throw new ArgumentOutOfRangeException(nameof(timeout));
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);
        try
        {
            return await operation(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCancellation.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation exceeded {timeout}.");
        }
    }

    private static async Task<T> RunSyncOperationAsync<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationTokenSource siblings)
    {
        try { return await operation(siblings.Token).ConfigureAwait(false); }
        catch { TryCancelSilently(siblings); throw; }
    }

    private static async ValueTask ObserveAllAsync(IEnumerable<Task> tasks)
    {
        try { await Task.WhenAll(tasks).ConfigureAwait(false); }
        catch { }
    }

    private static void ValidateOperations<TDelegate>(TDelegate[] operations) where TDelegate : Delegate
    {
        for (int index = 0; index < operations.Length; index++)
            ArgumentNullException.ThrowIfNull(operations[index], $"operations[{index}]");
    }

    private static void TryCancelSilently(CancellationTokenSource cancellation)
    {
        try { cancellation.Cancel(); }
        catch (AggregateException) { }
    }
}
