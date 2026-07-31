namespace CShrimpSharp.Transactions;

/// <summary>
/// Describes the lifecycle of a <see cref="ShrimpTransaction" />.
/// </summary>
public enum TransactionState
{
    /// <summary>The transaction accepts new compensation actions.</summary>
    Active = 0,

    /// <summary>The transaction is currently executing compensation actions.</summary>
    RollingBack = 1,

    /// <summary>The transaction completed successfully and discarded its compensations.</summary>
    Committed = 2,

    /// <summary>The transaction executed all compensation actions successfully.</summary>
    RolledBack = 3,

    /// <summary>One or more compensation actions failed.</summary>
    Faulted = 4,
}
