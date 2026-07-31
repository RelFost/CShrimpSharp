namespace CShrimpSharp.Transactions;

public sealed class ShrimpTransaction : IAsyncDisposable
{
    private readonly Stack<Func<CancellationToken,ValueTask>> _rollback=[];
    private bool _completed;
    public void OnRollback(Func<CancellationToken,ValueTask> action)
    {
        if(_completed) throw new InvalidOperationException("Transaction is completed.");
        _rollback.Push(action ?? throw new ArgumentNullException(nameof(action)));
    }
    public void OnRollback(Action action) => OnRollback(_=>{ action(); return ValueTask.CompletedTask; });
    public async ValueTask StepAsync(Func<CancellationToken,ValueTask> operation, Func<CancellationToken,ValueTask> rollback, CancellationToken cancellationToken=default)
    {
        if(_completed) throw new InvalidOperationException("Transaction is completed.");
        await operation(cancellationToken).ConfigureAwait(false);
        OnRollback(rollback);
    }
    public void Commit() { if(_completed) throw new InvalidOperationException("Transaction is completed."); _rollback.Clear(); _completed=true; }
    public async ValueTask RollbackAsync(CancellationToken cancellationToken=default)
    {
        if(_completed) return;
        List<Exception>? errors=null;
        while(_rollback.TryPop(out var action)) { try { await action(cancellationToken).ConfigureAwait(false); } catch(Exception ex) { (errors??=[]).Add(ex); } }
        _completed=true;
        if(errors is not null) throw new AggregateException("One or more compensations failed.",errors);
    }
    public async ValueTask DisposeAsync() { if(!_completed) await RollbackAsync(CancellationToken.None).ConfigureAwait(false); }
}
