namespace CShrimpSharp.Transactions;

/// <summary>Describes the lifecycle state of a compensating transaction.</summary>
public enum ShrimpTransactionState
{
    /// <summary>The transaction accepts steps and compensations.</summary>
    Active,

    /// <summary>The transaction was committed.</summary>
    Committed,

    /// <summary>The transaction was rolled back successfully.</summary>
    RolledBack,

    /// <summary>Rollback completed with one or more compensation failures.</summary>
    RollbackFailed,
}
