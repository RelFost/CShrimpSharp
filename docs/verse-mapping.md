# Mapping selected Verse ideas to CShrimpSharp

CShrimpSharp is inspired by programming ideas visible in Verse documentation. The mapping is conceptual rather than source-compatible.

## Failable operations

Verse can use failure as part of expression evaluation and failure contexts. CShrimpSharp represents this explicitly:

```csharp
Result<Player, Failure> player = FindPlayer(id);

Result<Item, Failure> equipped = player
    .Bind(current => current.Inventory.Find(itemId))
    .Ensure(
        item => item.CanEquip,
        item => new Failure("cannot_equip", $"Cannot equip {item.Name}."));
```

Differences:

- C# control flow is unchanged;
- callers compose `Result` and `Option` values through methods;
- the compiler does not automatically reject calls with incompatible effects;
- arbitrary C# expressions do not become failable expressions.

## `sync`

Conceptual mapping:

```text
Verse sync  →  Shrimp.SyncAsync
```

CShrimpSharp starts all supplied delegates and waits for every child. Value results retain input order. Failure requests cancellation of siblings, but siblings must cooperate.

## `race`

Conceptual mapping:

```text
Verse race  →  Shrimp.RaceAsync
```

The first completed child determines the result. Losers receive cancellation and are observed before the method returns. CShrimpSharp cannot instantly terminate code that ignores cancellation.

## `branch`

Conceptual mapping:

```text
Verse branch  →  ShrimpScope.Branch
```

A branch is explicitly registered with a `ShrimpScope`. The parent scope joins all registered branches. This is intentionally different from a fire-and-forget `Task.Run` call.

## Transactional rollback

Conceptual mapping:

```text
Verse rollback semantics  →  ShrimpTransaction compensation stack
```

A CShrimpSharp transaction does not record arbitrary assignments. The caller registers inverse actions:

```csharp
balance -= price;
transaction.OnRollback(() => balance += price);
```

This resembles a small local saga or compensation log more than language-level transactional memory.

## Effects

Verse can express effects as part of callable behavior. A normal C# library cannot extend the C# type system with equivalent effect checking.

The planned analyzer package may detect selected suspicious patterns, but analyzer diagnostics remain approximations and can be suppressed. The runtime API must therefore stay safe and understandable without analyzer enforcement.

## Determinism

CShrimpSharp does not guarantee deterministic ordering between concurrently running delegates. It guarantees only documented observations such as input-order result collection and reverse-order compensation. Scheduling, I/O timing, thread-pool behavior, and user code remain ordinary .NET behavior.

## Source compatibility

The project intentionally does not accept Verse source, generate Verse source, or promise API parity with any current or future Verse release. Names may be inspired by concepts, while behavior is defined solely by CShrimpSharp's own documentation and tests.
