namespace CShrimpSharp.Concurrency;

public static class Shrimp
{
    public static async ValueTask<IReadOnlyList<T>> SyncAsync<T>(IEnumerable<Func<CancellationToken,ValueTask<T>>> operations, CancellationToken cancellationToken=default)
    {
        Func<CancellationToken,ValueTask<T>>[] ops=[.. operations];
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks=ops.Select(op=>Run(op,linked)).ToArray();
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }
    public static ValueTask<IReadOnlyList<T>> SyncAsync<T>(params Func<CancellationToken,ValueTask<T>>[] operations) => SyncAsync(operations,CancellationToken.None);
    public static async ValueTask<(T1,T2)> SyncAsync<T1,T2>(Func<CancellationToken,ValueTask<T1>> first, Func<CancellationToken,ValueTask<T2>> second, CancellationToken cancellationToken=default)
    {
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T1> a=Run(first,linked); Task<T2> b=Run(second,linked);
        await Task.WhenAll(a,b).ConfigureAwait(false);
        return (await a.ConfigureAwait(false), await b.ConfigureAwait(false));
    }
    public static async ValueTask<RaceResult<T>> RaceAsync<T>(IEnumerable<Func<CancellationToken,ValueTask<T>>> operations, CancellationToken cancellationToken=default)
    {
        Func<CancellationToken,ValueTask<T>>[] ops=[.. operations];
        if(ops.Length==0) throw new ArgumentException("At least one operation is required.",nameof(operations));
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<T>[] tasks=ops.Select(op=>op(linked.Token).AsTask()).ToArray();
        Task<T> winner=await Task.WhenAny(tasks).ConfigureAwait(false);
        int index=Array.IndexOf(tasks,winner); linked.Cancel();
        try { return new(index,await winner.ConfigureAwait(false)); }
        finally { try { await Task.WhenAll(tasks).ConfigureAwait(false); } catch { } }
    }
    public static ValueTask<RaceResult<T>> RaceAsync<T>(params Func<CancellationToken,ValueTask<T>>[] operations) => RaceAsync(operations,CancellationToken.None);
    public static async ValueTask<T> WithTimeoutAsync<T>(Func<CancellationToken,ValueTask<T>> operation, TimeSpan timeout, CancellationToken cancellationToken=default)
    {
        using var timeoutCts=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        try { return await operation(timeoutCts.Token).ConfigureAwait(false); }
        catch(OperationCanceledException) when(!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested) { throw new TimeoutException($"Operation exceeded {timeout}."); }
    }
    private static async Task<T> Run<T>(Func<CancellationToken,ValueTask<T>> op, CancellationTokenSource siblings)
    {
        try { return await op(siblings.Token).ConfigureAwait(false); }
        catch { try { siblings.Cancel(); } catch(AggregateException) { } throw; }
    }
}
