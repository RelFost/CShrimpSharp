namespace CShrimpSharp.Transactions;

/// <summary>
/// The exception thrown when one or more transaction compensation actions fail.
/// </summary>
public sealed class TransactionRollbackException : Exception
{
    /// <summary>
    /// Initializes a rollback exception.
    /// </summary>
    /// <param name="rollbackExceptions">All compensation failures in execution order.</param>
    public TransactionRollbackException(IEnumerable<Exception> rollbackExceptions)
        : this(Materialize(rollbackExceptions))
    {
    }

    private TransactionRollbackException(Exception[] rollbackExceptions)
        : base(
            $"Transaction rollback failed in {rollbackExceptions.Length} compensation action(s).",
            new AggregateException(rollbackExceptions))
    {
        RollbackExceptions = Array.AsReadOnly(rollbackExceptions);
    }

    /// <summary>
    /// Gets all compensation failures in execution order.
    /// </summary>
    public IReadOnlyList<Exception> RollbackExceptions { get; }

    private static Exception[] Materialize(IEnumerable<Exception> rollbackExceptions)
    {
        ArgumentNullException.ThrowIfNull(rollbackExceptions);

        Exception[] exceptions = [.. rollbackExceptions];

        if (exceptions.Length == 0)
        {
            throw new ArgumentException(
                "At least one rollback exception is required.",
                nameof(rollbackExceptions));
        }

        if (Array.Exists(exceptions, static exception => exception is null))
        {
            throw new ArgumentException(
                "Rollback exceptions cannot contain null values.",
                nameof(rollbackExceptions));
        }

        return exceptions;
    }
}
