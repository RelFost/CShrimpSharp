# Core types

## Option<T>

`Option<T>` represents either `Some(value)` or `None`. `Some(null)` is rejected.

```csharp
Option<string> nickname = Option.From<string>(null);
string displayName = nickname
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Guest");
```

Use `Bind` when the next operation already returns an option:

```csharp
Option<int> ParsePositive(string input) =>
    int.TryParse(input, out int value) && value > 0
        ? Option.Some(value)
        : Option.None<int>();

Option<int> value = Option.Some("42").Bind(ParsePositive);
```

Convert to a result when absence needs a reason:

```csharp
Result<string, Failure> required = Option.None<string>().ToResult(
    static () => new Failure("required", "A value is required."));
```

## Result<TValue,TError>

A result is explicitly initialized as success or failure. `default(Result<,>)` is invalid and throws when observed.

```csharp
Result<int, Failure> result = Result.Success(21)
    .Map(static value => value * 2)
    .Ensure(
        static value => value < 100,
        static value => new Failure("too_large", $"{value} is too large."));
```

Transform or recover from failures:

```csharp
Result<int, string> normalized = Result<int, Failure>
    .Failure(new Failure("missing", "Not found"))
    .MapError(static error => error.Code)
    .Recover(static code => code == "missing" ? 0 : -1);
```

Observe without changing the result:

```csharp
Result<int, Failure> traced = result
    .Tap(value => Console.WriteLine($"Success: {value}"))
    .TapError(error => Console.WriteLine($"Failure: {error}"));
```

## Validation<TValue,TError>

Use `Validation` when all independent errors should be accumulated instead of stopping at the first one.

```csharp
Validation<string, string> name = string.IsNullOrWhiteSpace(input)
    ? Validation<string, string>.Invalid("Name is required")
    : Validation<string, string>.Valid(input);
```

Use `Result` for sequential operations and `Validation` for independent input checks.
