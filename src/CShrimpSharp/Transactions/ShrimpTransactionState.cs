namespace CShrimpSharp.Transactions;

/// <summary>Describes the lifecycle state of a compensating transaction.</summary>
public enum ShrimpTransactionState
{
    /// <summary>The transaction accepts operations and compensation registrations.</summary>
    Active,
    /// <summary>The transaction completed successfully and compensations were discarded.</summary>
    Committed,
    /// <summary>Every registered compensation completed successfully.</summary>
    RolledBack,
    /// <summary>Rollback completed after one or more compensations failed.</summary>
    RollbackFailed,
}
