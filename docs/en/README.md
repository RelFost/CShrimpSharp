<div align="center">
  <img src="https://raw.githubusercontent.com/RelFost/CShrimpSharp/main/assets/icon.png" width="112" alt="CShrimpSharp icon">

# CShrimpSharp documentation

**Practical guides for explicit failures, structured concurrency, and compensating transactions.**

[Repository README](../../README.md) · [Русская документация](../ru/README.md) · [Discord](https://discord.gg/QsKC34GbHS)
</div>

> [!NOTE]
> These guides describe the `0.3.0-preview.1` development line targeting `.NET 8` and `.NET 10`.

## Start here

| Guide | Use it when you need to… |
| --- | --- |
| [Getting started](getting-started.md) | install the package, create the first pipeline, and understand the basic conventions. |
| [Core types](core.md) | choose between `Option`, `Result`, and `Validation`. |
| [Asynchronous composition](async-composition.md) | compose asynchronous mapping, binding, recovery, and side effects. |
| [Structured concurrency](concurrency.md) | run related work with sync, settled sync, race, successful race, cancellation, and timeout. |
| [Compensating transactions](transactions.md) | register rollback actions and manage transaction state. |

## Go deeper

| Guide | Focus |
| --- | --- |
| [API reference](api-reference.md) | Compact map of public types, methods, and common call patterns. |
| [Architecture and guarantees](architecture.md) | Behavioral contracts, null policy, cancellation, task observation, and design boundaries. |
| [Cookbook](cookbook.md) | Complete recipes for common application and game scenarios. |
| [Release process](releasing.md) | Build, test, pack, package smoke tests, Native AOT, NuGet, and GitHub Releases. |
| [Roadmap](roadmap.md) | Planned stabilization work before `1.0`. |

## Suggested reading paths

**Application developer:** Getting started → Core types → Async composition → Cookbook.

**Concurrency-heavy code:** Getting started → Structured concurrency → Architecture and guarantees.

**Library contributor:** Architecture and guarantees → Release process → Roadmap.
