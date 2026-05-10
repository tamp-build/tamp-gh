# Tamp.GitHubCli

GitHub CLI (`gh`) wrapper for [Tamp](https://github.com/tamp-build/tamp).

| Package | gh CLI | Status |
|---|---|---|
| [`Tamp.GitHubCli.V2`](src/Tamp.GitHubCli.V2) | 2.x | live |

Sub-facades by resource:

| Sub-facade | Verbs |
|---|---|
| `GhRelease` | `Create`, `Upload` |
| `GhPr` | `Create`, `List`, `View`, `Merge` |
| `GhIssue` | `Create`, `List`, `Close` |
| `GhApi` | `Request` (escape hatch for any REST / GraphQL endpoint) |

## Why a separate repo

`gh` ships every couple weeks with new subcommands and frequent flag
additions. Per the Tamp satellite-repo convention, third-party tools
with their own release cadence live outside main.

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

    [Secret("GitHub token", EnvironmentVariable = "GH_TOKEN")]
    readonly Secret GhToken = null!;

    AbsolutePath Artifacts => RootDirectory / "artifacts";
    [GitRepository] readonly GitRepository Git = null!;

    Target ReleasePackages => _ => _
        .DependsOn(nameof(Pack))
        .OnlyWhen(() => Git.Branch == "main")
        .Executes(() => GhRelease.Create(Gh, s => s
            .SetTag($"v{Version}")
            .SetTitle($"v{Version}")
            .SetGenerateNotes()
            .AddFiles(Artifacts.GlobFiles("*.nupkg"))
            .SetToken(GhToken)));
}
```

## Quick example — GitHub API call

```csharp
Target FetchLatestRelease => _ => _
    .Executes(() => GhApi.Request(Gh, s => s
        .SetEndpoint("repos/{owner}/{repo}/releases/latest")
        .SetJq(".tag_name")
        .SetToken(GhToken)));
```

## Auth

The wrapper accepts a `Secret` token via `SetToken(...)` which is set as the
`GH_TOKEN` environment variable on the spawned process and registered with
the runner's redaction table. You can also use `SetEnvironmentVariable("GH_TOKEN", ...)`
directly if you need to pass a non-`Secret` value (rare).

## License

[MIT](LICENSE) — same as `tamp` core. (gh itself is MIT.)
