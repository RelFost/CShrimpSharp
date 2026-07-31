# Recommended GitHub settings

## Repository presentation

Use the following repository description:

> Verse-inspired failable operations, structured concurrency, and compensating transactions for C# and .NET.

Recommended topics:

```text
csharp
dotnet
verse
result
option
structured-concurrency
transactions
async
functional-programming
game-development
```

Keep the repository public and use `main` as the default branch. Enable Issues and private vulnerability reporting. Discussions can be enabled once there is enough usage to separate questions from actionable issues.

## Merge policy

Recommended pull-request settings:

- allow squash merging;
- allow rebase merging;
- disable merge commits once the repository starts receiving external pull requests;
- automatically delete head branches;
- require branches to be up to date before merging.

Use the pull-request title as the squash commit subject and follow the commit examples in `CONTRIBUTING.md`.

## `main` ruleset

Create a branch ruleset targeting `main` with these minimum protections:

1. Require a pull request before merging.
2. Require one approval after the project has another regular contributor.
3. Dismiss stale approvals after new commits.
4. Require conversation resolution.
5. Require the `Build, test, and pack` status check from `.github/workflows/ci.yml`.
6. Block force pushes and branch deletion.
7. Permit repository administrators to bypass while the project has a single maintainer.

For the initial bootstrap commit, push directly to the empty repository first, then create the ruleset after the CI check name exists.

## Actions and secrets

The included workflows need read access to repository contents. The release workflow publishes only when its manual `publish_nuget` input is enabled. Before publishing, create a repository Actions secret named `NUGET_API_KEY` and scope the corresponding NuGet key to the `CShrimpSharp` package.

Dependabot is configured for NuGet and GitHub Actions updates. Create the `dependencies` and `github-actions` labels if GitHub does not create them automatically.

## Suggested first milestones

```text
0.1 — Core semantics
0.2 — Async composition
0.3 — Diagnostics and analyzers
1.0 — Stable contracts
```

The roadmap is directional; milestones should contain only issues accepted for implementation.
