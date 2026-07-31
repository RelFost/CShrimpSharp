# Основные типы

## Option<T>

`Option<T>` хранит либо `Some(value)`, либо `None`. Вызов `Some(null)` запрещён.

```csharp
Option<string> nickname = Option.From<string>(null);
string displayName = nickname
    .Map(static value => value.Trim())
    .Filter(static value => value.Length > 0)
    .GetValueOr("Гость");
```

`Bind` нужен, когда следующая операция уже возвращает `Option`:

```csharp
Option<int> ParsePositive(string input) =>
    int.TryParse(input, out int value) && value > 0
        ? Option.Some(value)
        : Option.None<int>();

Option<int> value = Option.Some("42").Bind(ParsePositive);
```

Преобразование отсутствия в объяснимую ошибку:

```csharp
Result<string, Failure> required = Option.None<string>().ToResult(
    static () => new Failure("required", "Значение обязательно."));
```

## Result<TValue,TError>

Результат явно создаётся как успех или ошибка. `default(Result<,>)` считается некорректным состоянием.

```csharp
Result<int, Failure> result = Result.Success(21)
    .Map(static value => value * 2)
    .Ensure(
        static value => value < 100,
        static value => new Failure("too_large", $"{value} слишком велико."));
```

Изменение и восстановление ошибки:

```csharp
Result<int, string> normalized = Result<int, Failure>
    .Failure(new Failure("missing", "Не найдено"))
    .MapError(static error => error.Code)
    .Recover(static code => code == "missing" ? 0 : -1);
```

Наблюдение без изменения результата:

```csharp
Result<int, Failure> traced = result
    .Tap(value => Console.WriteLine($"Успех: {value}"))
    .TapError(error => Console.WriteLine($"Ошибка: {error}"));
```

## Validation<TValue,TError>

`Validation` подходит для независимых проверок, когда нужно собрать все ошибки, а не остановиться на первой.

```csharp
Validation<string, string> name = string.IsNullOrWhiteSpace(input)
    ? Validation<string, string>.Invalid("Имя обязательно")
    : Validation<string, string>.Valid(input);
```

Для последовательной логики используй `Result`, для набора независимых проверок — `Validation`.
