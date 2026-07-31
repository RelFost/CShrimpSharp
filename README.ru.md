<div align="center">

# CShrimpSharp

**Вдохновлённые Verse явные ошибки, структурированная конкурентность и компенсирующие транзакции для современного C# и .NET.**

[English](README.md) · [Документация](docs/ru/README.md) · [Discord](https://discord.gg/QsKC34GbHS)

</div>

> [!IMPORTANT]
> CShrimpSharp — библиотека для C#, а не компилятор Verse, среда выполнения, слой совместимости или продукт Epic Games.

## Возможности

- `Option<T>` для явных необязательных значений.
- `Result<TValue, TError>` для ожидаемых ошибок как данных.
- `Validation<TValue, TError>` для накопления нескольких ошибок валидации.
- Композиция через `Map`, `Bind`, `Ensure`, LINQ, `Sequence` и `Traverse`.
- Безопасный доступ к спискам и словарям.
- `SyncAsync`, типизированные перегрузки, `RaceAsync`, тайм-ауты и управляемые области задач.
- Явные компенсирующие транзакции с откатом в порядке LIFO.

## Требования

- .NET 10 SDK
- C# 14

## Установка

После публикации в NuGet:

```powershell
dotnet add package CShrimpSharp --prerelease
```

## Сборка

```powershell
dotnet restore CShrimpSharp.sln
dotnet format CShrimpSharp.sln --verify-no-changes --no-restore
dotnet build CShrimpSharp.sln --configuration Release --no-restore
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --no-build --output artifacts/packages
```

## Документация

- [Русская документация](docs/ru/README.md)
- [English documentation](docs/en/README.md)
- [Публикация релизов и пакетов](docs/ru/releasing.md)
- [Дорожная карта](docs/ru/roadmap.md)

## Сообщество

Вопросы и обсуждение API: **[Discord CShrimp](https://discord.gg/QsKC34GbHS)**.

## Лицензия

[MIT](LICENSE). Дополнительная информация — в [NOTICE.md](NOTICE.md).
