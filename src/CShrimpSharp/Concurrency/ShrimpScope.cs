namespace CShrimpSharp.Concurrency;

/// <summary>
/// Owns a group of asynchronous branches so they cannot silently outlive their parent scope.
/// </summary>
public sealed class ShrimpScope : IAsyncDisposable
{
    private readonly object _gate = new();
    private readonly CancellationTokenSource _cancellation;
    private readonly CancellationToken _token;
    private readonly List<Task> _branches = [];
    private readonly ShrimpScopeOptions _options;

    private int _registrationsInProgress;
    private bool _joining;
    private bool _completed;
    private bool _disposeStarted;
    private bool _disposed;
    private Task? _disposeTask;

    /// <summary>
    /// Initializes a new structured-concurrency scope.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the entire scope.</param>
    /// <param name="options">Optional scope behavior.</param>
    public ShrimpScope(
        CancellationToken cancellationToken = default,
        ShrimpScopeOptions? options = null)
    {
        _options = options ?? ShrimpScopeOptions.Default;
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _token = _cancellation.Token;
    }

    /// <summary>
    /// Gets the cancellation token shared by all branches.
    /// </summary>
    public CancellationToken Token => _token;

    /// <summary>
    /// Gets the number of branches registered in this scope.
    /// </summary>
    public int BranchCount
    {
        get
        {
            lock (_gate)
            {
                return _branches.Count;
            }
        }
    }

    internal bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _disposed;
            }
        }
    }

    /// <summary>
    /// Starts and registers a child branch.
    /// </summary>
    /// <param name="operation">The branch operation.</param>
    /// <remarks>
    /// Register all branches before calling <see cref="JoinAsync" />. A joining scope is sealed
    /// and rejects new branches.
    /// </remarks>
    public void Branch(Func<CancellationToken, ValueTask> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        lock (_gate)
        {
            ThrowIfDisposedOrDisposing();

            if (_joining || _completed)
            {
                throw new InvalidOperationException(
                    "Cannot register a branch after the scope has started joining.");
            }

            _registrationsInProgress++;
        }

        Task branchTask;

        try
        {
            branchTask = RunBranchAsync(operation);
        }
        catch
        {
            lock (_gate)
            {
                _registrationsInProgress--;
            }

            throw;
        }

        lock (_gate)
        {
            _branches.Add(branchTask);
            _registrationsInProgress--;
        }
    }

    /// <summary>
    /// Cancels the scope and all cooperative branches.
    /// </summary>
    public void Cancel()
    {
        lock (_gate)
        {
            ThrowIfDisposedOrDisposing();
        }

        _cancellation.Cancel();
    }

    /// <summary>
    /// Seals the scope and waits for every registered branch.
    /// </summary>
    public ValueTask JoinAsync() =>
        JoinCoreAsync(suppressCancellation: false, suppressFailures: false);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => new(GetOrStartDisposeTask());

    internal async ValueTask DisposeSilentlyAsync()
    {
        try
        {
            await GetOrStartDisposeTask().ConfigureAwait(false);
        }
        catch
        {
            // Cleanup must not replace the exception already being propagated by the scope body.
        }
    }

    private async Task RunBranchAsync(Func<CancellationToken, ValueTask> operation)
    {
        try
        {
            await operation(Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (Token.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            if (_options.CancelSiblingsOnFailure)
            {
                TryCancelSilently();
            }

            throw;
        }
    }

    private async ValueTask JoinCoreAsync(
        bool suppressCancellation,
        bool suppressFailures)
    {
        lock (_gate)
        {
            if (_completed)
            {
                return;
            }

            _joining = true;
        }

        Task[] branches;

        while (true)
        {
            lock (_gate)
            {
                if (_registrationsInProgress == 0)
                {
                    branches = [.. _branches];
                    break;
                }
            }

            await Task.Yield();
        }

        try
        {
            await Task.WhenAll(branches).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (
            suppressCancellation && Token.IsCancellationRequested)
        {
        }
        catch when (suppressFailures)
        {
        }
        finally
        {
            lock (_gate)
            {
                _completed = true;
                _branches.Clear();
            }
        }
    }

    private Task GetOrStartDisposeTask()
    {
        TaskCompletionSource<object?> completion;

        lock (_gate)
        {
            if (_disposeTask is not null)
            {
                return _disposeTask;
            }

            _disposeStarted = true;
            completion = new TaskCompletionSource<object?>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _disposeTask = completion.Task;
        }

        _ = CompleteDisposeAsync(completion);
        return completion.Task;
    }

    private async Task CompleteDisposeAsync(TaskCompletionSource<object?> completion)
    {
        Exception? failure = null;

        try
        {
            TryCancelSilently();
            await JoinCoreAsync(
                suppressCancellation: true,
                suppressFailures: false).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        try
        {
            _cancellation.Dispose();
        }
        catch (Exception exception)
        {
            failure = failure is null
                ? exception
                : new AggregateException(failure, exception);
        }

        lock (_gate)
        {
            _disposed = true;
        }

        if (failure is null)
        {
            completion.SetResult(null);
        }
        else
        {
            completion.SetException(failure);
        }
    }

    private void TryCancelSilently()
    {
        try
        {
            _cancellation.Cancel();
        }
        catch (AggregateException)
        {
            // A cancellation callback must not replace the branch failure that caused cancellation.
        }
        catch (ObjectDisposedException)
        {
            // Concurrent disposal has already completed the scope.
        }
    }

    private void ThrowIfDisposedOrDisposing()
    {
        if (_disposed || _disposeStarted)
        {
            throw new ObjectDisposedException(nameof(ShrimpScope));
        }
    }
}
