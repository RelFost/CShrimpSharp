# Release process

Releases are intentionally manual during preview.

## 1. Prepare the repository

1. Move completed changelog entries from `Unreleased` into a version section.
2. Update `VersionPrefix` and `VersionSuffix` in `src/CShrimpSharp/CShrimpSharp.csproj` when the default development version changes.
3. Run the full build script.

```powershell
./scripts/build.ps1
```

## 2. Validate the package

```powershell
dotnet pack src/CShrimpSharp/CShrimpSharp.csproj `
    --configuration Release `
    --output artifacts/packages `
    -p:PackageVersion=0.1.0-preview.1
```

Inspect both the `.nupkg` and `.snupkg`, including README, icon, XML documentation, repository metadata, and dependencies.

## 3. Configure NuGet publishing

Create a GitHub Actions repository secret named exactly:

```text
NUGET_API_KEY
```

Restrict the NuGet key to the `CShrimpSharp` package and use the shortest practical expiration.

## 4. Run the release workflow

Open **Actions → Release → Run workflow**.

- Enter a package version without a leading `v`, for example `0.1.0-preview.1`.
- Keep **Publish package to NuGet** disabled for a packaging dry run.
- Enable it only after inspecting the dry-run artifact.

The workflow always builds, tests, and uploads the package artifact. NuGet publishing runs only when explicitly enabled.

## 5. Create the GitHub release

After successful NuGet publication, create a Git tag and GitHub release using the matching `v`-prefixed version:

```powershell
git tag -a v0.1.0-preview.1 -m "CShrimpSharp 0.1.0-preview.1"
git push origin v0.1.0-preview.1
```

Use the matching changelog section as the release notes.
