namespace CShrimpSharp;

public readonly struct Validation<TValue,TError>
{
    private readonly TValue? _value;
    private readonly IReadOnlyList<TError>? _errors;
    private Validation(TValue value) { _value=value; _errors=null; }
    private Validation(IReadOnlyList<TError> errors) { _value=default; _errors=errors; }
    public bool IsValid => _errors is null;
    public TValue Value => IsValid ? _value! : throw new InvalidOperationException("Validation is invalid.");
    public IReadOnlyList<TError> Errors => _errors ?? Array.Empty<TError>();
    public static Validation<TValue,TError> Valid(TValue value) { ArgumentNullException.ThrowIfNull(value); return new(value); }
    public static Validation<TValue,TError> Invalid(params TError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if(errors.Length==0) throw new ArgumentException("At least one error is required.", nameof(errors));
        return new(Array.AsReadOnly(errors));
    }
    public TResult Match<TResult>(Func<TValue,TResult> valid, Func<IReadOnlyList<TError>,TResult> invalid) => IsValid ? valid(_value!) : invalid(_errors!);
}
