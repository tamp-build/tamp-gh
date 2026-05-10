namespace Tamp.GitHubCli.V2;

/// <summary>
/// Common knobs supported by every <c>gh</c> verb.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Repo"/> (<c>--repo OWNER/REPO</c>) is supported by every
/// resource verb (release / pr / issue) but NOT by <c>gh api</c>, which
/// uses <see cref="Hostname"/> instead and embeds the path directly.
/// </para>
/// <para>
/// Authentication is via the <c>GH_TOKEN</c> / <c>GITHUB_TOKEN</c>
/// environment variable — set it through <see cref="EnvironmentVariables"/>
/// (typically passing a <see cref="Secret"/> value). gh handles the rest.
/// </para>
/// </remarks>
public abstract class GhCommonSettings
{
    /// <summary>Override the active repo. Maps to <c>--repo OWNER/REPO</c> (or <c>HOST/OWNER/REPO</c>).</summary>
    public string? Repo { get; set; }

    /// <summary>Working directory of the spawned process.</summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>Per-invocation environment variables. <c>GH_TOKEN</c> is the standard token.</summary>
    public Dictionary<string, string> EnvironmentVariables { get; } = new();

    /// <summary>Token to inject as <c>GH_TOKEN</c>. Pass as <see cref="Secret"/> so it's redacted in logs.</summary>
    public Secret? Token { get; set; }

    protected void EmitRepoArgument(List<string> args)
    {
        if (!string.IsNullOrEmpty(Repo)) { args.Add("--repo"); args.Add(Repo!); }
    }

    protected internal Dictionary<string, string> BuildEnvironment()
    {
        var env = new Dictionary<string, string>(EnvironmentVariables);
        if (Token is { } t) env["GH_TOKEN"] = t.Reveal();
        return env;
    }

    protected internal IReadOnlyList<Secret> BuildSecrets()
        => Token is null ? Array.Empty<Secret>() : new[] { Token };
}

/// <summary>
/// Generic fluent helpers for the common knobs. Each settings subclass
/// inherits these via its own typed setter overrides (so the chain stays
/// in the subclass type for IntelliSense).
/// </summary>
public static class GhCommonSettingsExtensions
{
    public static T SetRepo<T>(this T s, string? repo) where T : GhCommonSettings { s.Repo = repo; return s; }
    public static T SetWorkingDirectory<T>(this T s, string? cwd) where T : GhCommonSettings { s.WorkingDirectory = cwd; return s; }
    public static T SetEnvironmentVariable<T>(this T s, string name, string value) where T : GhCommonSettings { s.EnvironmentVariables[name] = value; return s; }
    public static T SetToken<T>(this T s, Secret token) where T : GhCommonSettings { s.Token = token; return s; }
}
