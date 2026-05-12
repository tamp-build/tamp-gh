using System.IO;
using System.Text.Json;
using Tamp;
using Xunit;
using Xunit.Abstractions;

namespace Tamp.GitHubCli.V2.IntegrationTests;

/// <summary>
/// Real-tool exercises of the wrapper. Read-only paths only — we don't
/// have write auth in CI and don't want to create test artifacts in
/// real repos.
/// </summary>
public sealed class GhIntegrationTests
{
    private readonly ITestOutputHelper _output;
    public GhIntegrationTests(ITestOutputHelper output) => _output = output;

    private static Tool ResolveTool()
    {
        // Walk PATH — handles every OS/install combo (Homebrew on macOS, apt/dnf on
        // Linux, the GitHub-Runners pre-installed gh on every runner image, winget
        // / Chocolatey / Program Files on Windows).
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        var executable = OperatingSystem.IsWindows() ? "gh.exe" : "gh";
        foreach (var dir in pathVar.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir, executable);
            if (File.Exists(candidate)) return new Tool(AbsolutePath.Create(candidate));
        }
        throw new InvalidOperationException(
            "gh not found on PATH. Install: https://cli.github.com/");
    }

    private CaptureResult Run(CommandPlan plan)
    {
        _output.WriteLine($"$ {plan.Executable} {string.Join(' ', plan.Arguments)}");
        var result = ProcessRunner.Capture(plan);
        foreach (var line in result.Lines)
            _output.WriteLine($"  [{line.Type}] {line.Text}");
        _output.WriteLine($"  → exit {result.ExitCode}");
        return result;
    }

    [Fact]
    public void GhApi_Public_Endpoint_Returns_JSON_With_Expected_Field()
    {
        // Public unauthenticated endpoint (rate-limited to 60/h but
        // sufficient for one test). cli/cli is the gh project itself.
        var plan = GhApi.Request(ResolveTool(), s => s
            .SetEndpoint("repos/cli/cli")
            .SetJq(".name"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        // --jq .name pulls just the repo name out, no JSON envelope.
        Assert.Equal("cli", result.StdoutText.Trim('"', '\n', '\r', ' '));
    }

    [Fact]
    public void GhApi_Public_Endpoint_With_Headers_Round_Trips()
    {
        var plan = GhApi.Request(ResolveTool(), s => s
            .SetEndpoint("repos/cli/cli")
            .AddHeader("X-GitHub-Api-Version", "2022-11-28")
            .SetJq(".full_name"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("cli/cli", result.StdoutText);
    }

    [Fact]
    public void GhRelease_Create_DryRun_Validates_Wrapper_Without_Writing()
    {
        // gh release create has no --dry-run; the safest "validate the
        // command shape" test is to use an obviously-invalid tag against
        // a non-existent repo and confirm we get a sensible non-zero
        // exit. The wrapper-built command must reach gh successfully —
        // gh's own error proves the wrapper produced valid argv.
        var plan = GhRelease.Create(ResolveTool(), s => s
            .SetTag("tamp-test-tag-DOES-NOT-EXIST-9999")
            .SetTitle("Test")
            .SetNotes("Test")
            .SetRepo("cli/no-such-repo-for-tamp-test"));
        var result = Run(plan);
        // Must exit non-zero because the repo doesn't exist.
        Assert.NotEqual(0, result.ExitCode);
        // gh's stderr should mention something about the repo or auth —
        // the exact wording varies. We just confirm gh actually ran (no
        // "command not found" / argv parse error).
        var combined = result.StdoutText + "\n" + result.StderrText;
        Assert.True(
            combined.Contains("repository", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("gh auth", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("HTTP 404", StringComparison.OrdinalIgnoreCase),
            $"Expected gh to reach API and surface a repo / auth error. Got: {combined}");
    }

    [Fact]
    public void GhPr_List_Public_Repo_Returns_JSON_When_Json_Fields_Set()
    {
        var plan = GhPr.List(ResolveTool(), s => s
            .SetRepo("cli/cli")
            .SetState(GhPrState.Open)
            .SetLimit(3)
            .AddJsonField("number")
            .AddJsonField("title"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        // --json with fields produces a JSON array.
        var json = result.StdoutText.Trim();
        Assert.True(json.StartsWith('[') && json.EndsWith(']'),
            $"Expected JSON array, got: {json[..Math.Min(200, json.Length)]}");
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetArrayLength() <= 3);
        if (doc.RootElement.GetArrayLength() > 0)
        {
            var first = doc.RootElement[0];
            Assert.True(first.TryGetProperty("number", out _));
            Assert.True(first.TryGetProperty("title", out _));
        }
    }

    [Fact]
    public void GhIssue_List_Public_Repo_Returns_JSON()
    {
        var plan = GhIssue.List(ResolveTool(), s => s
            .SetRepo("cli/cli")
            .SetState(GhIssueState.Open)
            .SetLimit(2)
            .AddJsonField("number"));
        var result = Run(plan);
        Assert.Equal(0, result.ExitCode);
        var json = result.StdoutText.Trim();
        Assert.True(json.StartsWith('['));
    }
}
