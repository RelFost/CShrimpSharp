# Contributing to CShrimpSharp

Thank you for helping improve CShrimpSharp.

## Before opening an issue

1. Search existing issues and discussions.
2. Confirm the behavior on the current `main` branch.
3. Reduce bugs to the smallest reproducible example.
4. Separate API proposals from implementation pull requests whenever the design is not yet agreed.

Security-sensitive findings must follow [SECURITY.md](SECURITY.md), not a public issue.

## Local setup

```powershell
git clone https://github.com/RelFost/CShrimpSharp.git
cd CShrimpSharp
dotnet restore CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
```

The repository pins the expected SDK in `global.json`.

## Pull request rules

- Keep each pull request focused on one behavior or design change.
- Add tests for every externally visible behavior change.
- Preserve cancellation, failure, and rollback semantics documented under `docs/`.
- Update XML documentation and README examples when public APIs change.
- Add an entry under `Unreleased` in `CHANGELOG.md`.
- Run formatting, build, tests, and package creation before requesting review.

```powershell
dotnet format CShrimpSharp.sln
dotnet build CShrimpSharp.sln --configuration Release
dotnet test CShrimpSharp.sln --configuration Release --no-build
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --output artifacts/packages
```

## API design expectations

Public APIs should:

- make ownership and cancellation explicit;
- avoid hidden global state;
- avoid exceptions for expected domain failures;
- preserve the original failure unless a caller explicitly maps it;
- document whether child work is joined, cancelled, observed, or detached;
- document rollback order and behavior when a compensation fails;
- prefer `ValueTask` only where the implementation can reasonably complete synchronously.

Breaking public API changes are acceptable during preview, but they must be intentional and documented.

## Commit messages

Use a concise imperative subject. Conventional Commit prefixes are encouraged:

```text
feat: add timeout-aware race helper
fix: close branch registration race
docs: clarify compensation guarantees
test: cover rollback failure aggregation
```

## License

By contributing, you agree that your contribution is licensed under the repository's MIT License.
