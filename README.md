# Tamp.GitHubCli

GitHub CLI (`gh`) wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | gh CLI | Status |
|---|---|---|
| [`Tamp.GitHubCli.V2`](src/Tamp.GitHubCli.V2) | 2.x | live (0.1.0) |

Sub-facades by resource:

| Sub-facade | Verbs |
|---|---|
| `GhRelease` | `Create`, `Upload` |
| `GhPr` | `Create`, `List`, `View`, `Merge` |
| `GhIssue` | `Create`, `List`, `Close` |
| `GhApi` | `Request` (escape hatch for any REST / GraphQL endpoint) |

Tokens are typed as `Secret`, set as `GH_TOKEN` on the spawned process,
and registered with the runner's redaction table.

Requires `Tamp.Core ≥ 1.0.0`.

This was the **second satellite released through the Tamp dogfood
pipeline** — `dotnet tamp Ci` + `dotnet tamp Push` in
[`.github/workflows/release.yml`](.github/workflows/release.yml).

## Why a separate repo

`gh` ships every couple weeks with new subcommands and frequent flag
additions. Per the satellite-repo convention, third-party tools with
their own release cadence live outside main.

## Install

In your build script's `Directory.Packages.props`:

```xml
<PackageVersion Include="Tamp.GitHubCli.V2" Version="0.1.0" />
```

In `build/Build.csproj`:

```xml
<PackageReference Include="Tamp.GitHubCli.V2" />
```

## Quick example — release pipeline

```csharp
using Tamp;
using Tamp.GitHubCli.V2;
using Tamp.NetCli.V10;

class Build : TampBuild
{
    public static int Main(string[] args) => Execute<Build>(args);

    [NuGetPackage("gh", UseSystemPath = true)]
    readonly Tool Gh = null!;

    // Until TAM-78 lands [Secret] env-var resolution in Tamp.Core 1.0.1.
    static readonly Secret? GhToken =
        Environment.GetEnvironmentVariable("GH_TOKEN") is { Length: > 0 } v
            ? new Secret("GitHub token", v) : null;

    AbsolutePath Artifacts => RootDirectory / "artifacts";
    [GitRepository] readonly GitRepository Git = null!;

    Target ReleasePackages => _ => _
        .DependsOn(nameof(Pack))
        .OnlyWhen(() => Git.Branch == "main")
        .Requires(() => GhToken != null)
        .Executes(() => GhRelease.Create(Gh, s => s
            .SetTag($"v{Version}")
            .SetTitle($"v{Version}")
            .SetGenerateNotes()
            .AddFiles(Artifacts.GlobFiles("*.nupkg"))
            .SetToken(GhToken!)));
}
```

## Quick example — GitHub API call

```csharp
Target FetchLatestRelease => _ => _
    .Requires(() => GhToken != null)
    .Executes(() => GhApi.Request(Gh, s => s
        .SetEndpoint("repos/{owner}/{repo}/releases/latest")
        .SetJq(".tag_name")
        .SetToken(GhToken!)));
```

## Auth

The wrapper accepts a `Secret` token via `SetToken(...)`. The value is
set as the `GH_TOKEN` environment variable on the spawned process (gh
reads it from env, never from a flag — so the token never lands in the
OS process table). The same `Secret` is added to `plan.Secrets` for the
runner's redaction table.

For unauthenticated public-API reads (rate-limited to 60/h), omit the
token entirely.

## See also

- [tamp](https://github.com/tamp-build/tamp) — the core framework
- [gh CLI manual](https://cli.github.com/manual/) — verb reference

## License

[MIT](LICENSE) — same as `tamp` core. (gh itself is MIT.)
