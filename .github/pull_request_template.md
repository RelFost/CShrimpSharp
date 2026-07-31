## Summary

Describe the behavior changed by this pull request.

## Semantics checklist

- [ ] Failure behavior is documented and tested.
- [ ] Cancellation and child-task lifetime are documented and tested.
- [ ] Rollback order and rollback-failure behavior are documented and tested when applicable.
- [ ] Public XML documentation and README examples are updated when applicable.
- [ ] `CHANGELOG.md` contains an `Unreleased` entry.

## Validation

- [ ] `dotnet format CShrimpSharp.sln --verify-no-changes`
- [ ] `dotnet build CShrimpSharp.sln --configuration Release`
- [ ] `dotnet test CShrimpSharp.sln --configuration Release --no-build`
- [ ] `dotnet pack src/CShrimpSharp/CShrimpSharp.csproj --configuration Release --output artifacts/packages`
