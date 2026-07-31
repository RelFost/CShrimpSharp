namespace CShrimpSharp;

/// <summary>
/// Represents either a valid value or one or more accumulated validation errors.
/// </summary>
/// <typeparam name="TValue">The valid value type.</typeparam>
/// <typeparam name="TError">The validation error type.</typeparam>
public readonly struct Validation<TValue, TError>
{
    private readonly TValue? _value;
    private readonly IReadOnlyList<TError>? _errors;

    private Validation(TValue value)
    {
        _value = value;
        _errors = null;
    }

    private Validation(IReadOnlyList<TError> errors)
    {
        _value = default;
        _errors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid => _errors is null;

    /// <summary>
    /// Gets the valid value.
    /// </summary>
    public TValue Value => IsValid
        ? _value!
        : throw new InvalidOperationException("Validation is invalid.");

    /// <summary>
    /// Gets the accumulated errors, or an empty collection for a valid value.
    /// </summary>
    public IReadOnlyList<TError> Errors => _errors ?? Array.Empty<TError>();

    /// <summary>
    /// Creates a successful validation.
    /// </summary>
    public static Validation<TValue, TError> Valid(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Validation<TValue, TError>(value);
    }

    /// <summary>
    /// Creates a failed validation containing one or more errors.
    /// </summary>
    public static Validation<TValue, TError> Invalid(params TError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Length == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        return new Validation<TValue, TError>(Array.AsReadOnly(errors));
    }

    /// <summary>
    /// Produces one value from either validation branch.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> valid,
        Func<IReadOnlyList<TError>, TResult> invalid)
    {
        ArgumentNullException.ThrowIfNull(valid);
        ArgumentNullException.ThrowIfNull(invalid);
        return IsValid ? valid(_value!) : invalid(_errors!);
    }
}
