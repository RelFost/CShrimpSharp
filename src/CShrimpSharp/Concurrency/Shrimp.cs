namespace CShrimpSharp.Concurrency;

/// <summary>Provides Verse-inspired structured-concurrency operations for .NET tasks.</summary>
public static class Shrimp
{
    public static async ValueTask<IReadOnlyList<T>> SyncAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks = operationArray.Select(operation => RunSyncOperationAsync(operation, linkedCancellation)).ToArray();
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public static ValueTask<IReadOnlyList<T>> SyncAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        SyncAsync(operations, CancellationToken.None);

    public static async ValueTask<IReadOnlyList<Result<T, Exception>>> SyncSettledAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateOperations(operationArray);

        Task<Result<T, Exception>>[] tasks = operationArray.Select(async operation =>
        {
            try
            {
                return Result<T, Exception>.Success(await operation(cancellationToken).ConfigureAwait(false));
            }
            catch (Exception exception)
            {
                return Result<T, Exception>.Failure(exception);
            }
        }).ToArray();

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public static ValueTask<IReadOnlyList<Result<T, Exception>>> SyncSettledAsync<T>(
        params Func<CancellationToken, ValueTask<T>>[] operations) =>
        SyncSettledAsync(operations, CancellationToken.None);

    public static async ValueTask<(T1 First, T2 Second)> SyncAsync<T1, T2>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T1> a = RunSyncOperationAsync(first, linked);
        Task<T2> b = RunSyncOperationAsync(second, linked);
        await Task.WhenAll(a, b).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false));
    }

    public static async ValueTask<(T1 First, T2 Second, T3 Third)> SyncAsync<T1, T2, T3>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T1> a = RunCheckedAsync(first, linked, nameof(first));
        Task<T2> b = RunCheckedAsync(second, linked, nameof(second));
        Task<T3> c = RunCheckedAsync(third, linked, nameof(third));
        await Task.WhenAll(a, b, c).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false));
    }

    public static async ValueTask<(T1 First, T2 Second, T3 Third, T4 Fourth)> SyncAsync<T1, T2, T3, T4>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        Func<CancellationToken, ValueTask<T4>> fourth,
        CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T1> a = RunCheckedAsync(first, linked, nameof(first));
        Task<T2> b = RunCheckedAsync(second, linked, nameof(second));
        Task<T3> c = RunCheckedAsync(third, linked, nameof(third));
        Task<T4> d = RunCheckedAsync(fourth, linked, nameof(fourth));
        await Task.WhenAll(a, b, c, d).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false), await d.ConfigureAwait(false));
    }

    public static async ValueTask<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth)> SyncAsync<T1, T2, T3, T4, T5>(
        Func<CancellationToken, ValueTask<T1>> first,
        Func<CancellationToken, ValueTask<T2>> second,
        Func<CancellationToken, ValueTask<T3>> third,
        Func<CancellationToken, ValueTask<T4>> fourth,
        Func<CancellationToken, ValueTask<T5>> fifth,
        CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T1> a = RunCheckedAsync(first, linked, nameof(first));
        Task<T2> b = RunCheckedAsync(second, linked, nameof(second));
        Task<T3> c = RunCheckedAsync(third, linked, nameof(third));
        Task<T4> d = RunCheckedAsync(fourth, linked, nameof(fourth));
        Task<T5> e = RunCheckedAsync(fifth, linked, nameof(fifth));
        await Task.WhenAll(a, b, c, d, e).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false), await c.ConfigureAwait(false), await d.ConfigureAwait(false), await e.ConfigureAwait(false));
    }

    public static async ValueTask<RaceResult<T>> RaceAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateNonEmpty(operationArray);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks = operationArray.Select(operation => operation(linked.Token).AsTask()).ToArray();
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

    public static ValueTask<RaceResult<T>> RaceAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        RaceAsync(operations, CancellationToken.None);

    public static async ValueTask<RaceResult<T>> RaceSuccessAsync<T>(
        IEnumerable<Func<CancellationToken, ValueTask<T>>> operations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Func<CancellationToken, ValueTask<T>>[] operationArray = [.. operations];
        ValidateNonEmpty(operationArray);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        List<(int Index, Task<T> Task)> remaining = operationArray
            .Select((operation, index) => (index, operation(linked.Token).AsTask()))
            .ToList();
        List<Exception> errors = [];

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
            catch (Exception exception)
            {
                errors.Add(exception);
            }
        }

        throw new AggregateException("All raced operations failed.", errors);
    }

    public static ValueTask<RaceResult<T>> RaceSuccessAsync<T>(params Func<CancellationToken, ValueTask<T>>[] operations) =>
        RaceSuccessAsync(operations, CancellationToken.None);

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

    private static Task<T> RunCheckedAsync<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationTokenSource linked, string name)
    {
        ArgumentNullException.ThrowIfNull(operation, name);
        return RunSyncOperationAsync(operation, linked);
    }

    private static async Task<T> RunSyncOperationAsync<T>(Func<CancellationToken, ValueTask<T>> operation, CancellationTokenSource siblingCancellation)
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
        }
    }

    private static void ValidateNonEmpty<TDelegate>(TDelegate[] operations) where TDelegate : Delegate
    {
        ValidateOperations(operations);
        if (operations.Length == 0)
        {
            throw new ArgumentException("At least one operation is required.", nameof(operations));
        }
    }

    private static void ValidateOperations<TDelegate>(TDelegate[] operations) where TDelegate : Delegate
    {
        for (int index = 0; index < operations.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(operations[index], $"operations[{index}]");
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
        }
    }
}
