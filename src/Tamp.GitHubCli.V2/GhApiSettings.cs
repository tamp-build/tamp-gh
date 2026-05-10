namespace Tamp.GitHubCli.V2;

/// <summary>HTTP method for <c>gh api</c>.</summary>
public enum GhApiMethod
{
    Get,
    Post,
    Put,
    Patch,
    Delete,
    Head,
}

/// <summary>
/// Settings for <c>gh api &lt;endpoint&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// Use this when the resource verbs (<see cref="GhRelease"/>,
/// <see cref="GhPr"/>, <see cref="GhIssue"/>) don't cover what you
/// need — it's the escape hatch for any GitHub REST or GraphQL call.
/// </para>
/// <para>
/// Endpoint placeholders (<c>{owner}</c>, <c>{repo}</c>, <c>{branch}</c>)
/// get filled in automatically by gh from the active repo context.
/// </para>
/// </remarks>
public sealed class GhApiSettings : GhCommonSettings
{
    /// <summary>API endpoint (e.g. <c>repos/{owner}/{repo}/releases</c> or <c>graphql</c>). Required.</summary>
    public string? Endpoint { get; set; }

    /// <summary>HTTP method. Maps to <c>--method</c>. Default GET.</summary>
    public GhApiMethod? Method { get; set; }

    /// <summary>Typed parameters. Repeated as <c>--field key=value</c>. Numbers / true / false / null get JSON-coerced.</summary>
    public Dictionary<string, string> Fields { get; } = new();

    /// <summary>String-only parameters. Repeated as <c>--raw-field key=value</c>. No coercion.</summary>
    public Dictionary<string, string> RawFields { get; } = new();

    /// <summary>HTTP headers. Repeated as <c>--header key:value</c>.</summary>
    public Dictionary<string, string> Headers { get; } = new();

    /// <summary>Path to a request-body file (use "-" for stdin). Maps to <c>--input</c>.</summary>
    public string? InputFile { get; set; }

    /// <summary>jq filter to apply to the response. Maps to <c>--jq</c>.</summary>
    public string? Jq { get; set; }

    /// <summary>Go template for response formatting. Maps to <c>--template</c>.</summary>
    public string? Template { get; set; }

    /// <summary>Auto-paginate. Maps to <c>--paginate</c>.</summary>
    public bool Paginate { get; set; }

    /// <summary>Combine paginated responses into a JSON array. Maps to <c>--slurp</c>.</summary>
    public bool Slurp { get; set; }

    /// <summary>Include HTTP status + headers in output. Maps to <c>--include</c>.</summary>
    public bool IncludeHeaders { get; set; }

    /// <summary>Suppress response body output. Maps to <c>--silent</c>.</summary>
    public bool Silent { get; set; }

    /// <summary>Cache responses for the given duration. Maps to <c>--cache</c>.</summary>
    public string? Cache { get; set; }

    /// <summary>GitHub Enterprise hostname. Maps to <c>--hostname</c>.</summary>
    public string? Hostname { get; set; }

    /// <summary>API previews to opt into. Repeated as <c>--preview &lt;name&gt;</c>.</summary>
    public List<string> Previews { get; } = [];

    /// <summary>Verbose mode (full request + response). Maps to <c>--verbose</c>.</summary>
    public bool Verbose { get; set; }

    public GhApiSettings SetEndpoint(string? endpoint) { Endpoint = endpoint; return this; }
    public GhApiSettings SetMethod(GhApiMethod method) { Method = method; return this; }
    public GhApiSettings AddField(string key, string value) { Fields[key] = value; return this; }
    public GhApiSettings AddRawField(string key, string value) { RawFields[key] = value; return this; }
    public GhApiSettings AddHeader(string key, string value) { Headers[key] = value; return this; }
    public GhApiSettings SetInputFile(string? path) { InputFile = path; return this; }
    public GhApiSettings SetJq(string? query) { Jq = query; return this; }
    public GhApiSettings SetTemplate(string? template) { Template = template; return this; }
    public GhApiSettings SetPaginate(bool v = true) { Paginate = v; return this; }
    public GhApiSettings SetSlurp(bool v = true) { Slurp = v; return this; }
    public GhApiSettings SetIncludeHeaders(bool v = true) { IncludeHeaders = v; return this; }
    public GhApiSettings SetSilent(bool v = true) { Silent = v; return this; }
    public GhApiSettings SetCache(string? duration) { Cache = duration; return this; }
    public GhApiSettings SetHostname(string? hostname) { Hostname = hostname; return this; }
    public GhApiSettings AddPreview(string name) { Previews.Add(name); return this; }
    public GhApiSettings SetVerbose(bool v = true) { Verbose = v; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Endpoint)) throw new InvalidOperationException("gh api: Endpoint is required.");
        yield return "api";
        yield return Endpoint!;
        if (Method is { } m) { yield return "--method"; yield return m.ToString().ToUpperInvariant(); }
        foreach (var (k, v) in Fields) { yield return "--field"; yield return $"{k}={v}"; }
        foreach (var (k, v) in RawFields) { yield return "--raw-field"; yield return $"{k}={v}"; }
        foreach (var (k, v) in Headers) { yield return "--header"; yield return $"{k}:{v}"; }
        if (!string.IsNullOrEmpty(InputFile)) { yield return "--input"; yield return InputFile!; }
        if (!string.IsNullOrEmpty(Jq)) { yield return "--jq"; yield return Jq!; }
        if (!string.IsNullOrEmpty(Template)) { yield return "--template"; yield return Template!; }
        if (Paginate) yield return "--paginate";
        if (Slurp) yield return "--slurp";
        if (IncludeHeaders) yield return "--include";
        if (Silent) yield return "--silent";
        if (!string.IsNullOrEmpty(Cache)) { yield return "--cache"; yield return Cache!; }
        if (!string.IsNullOrEmpty(Hostname)) { yield return "--hostname"; yield return Hostname!; }
        foreach (var p in Previews) { yield return "--preview"; yield return p; }
        if (Verbose) yield return "--verbose";
        // gh api does NOT accept --repo; the endpoint embeds the repo.
    }
}
