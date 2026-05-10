namespace Tamp.GitHubCli.V2;

/// <summary>Settings for <c>gh pr create</c>.</summary>
public sealed class GhPrCreateSettings : GhCommonSettings
{
    /// <summary>PR title. Maps to <c>--title</c>.</summary>
    public string? Title { get; set; }

    /// <summary>PR body (inline). Maps to <c>--body</c>.</summary>
    public string? Body { get; set; }

    /// <summary>Path to a body file (use "-" for stdin). Maps to <c>--body-file</c>.</summary>
    public string? BodyFile { get; set; }

    /// <summary>Use commit info to fill title and body. Maps to <c>--fill</c>.</summary>
    public bool Fill { get; set; }

    /// <summary>Use only the first commit's info. Maps to <c>--fill-first</c>.</summary>
    public bool FillFirst { get; set; }

    /// <summary>Use commit message + body for description. Maps to <c>--fill-verbose</c>.</summary>
    public bool FillVerbose { get; set; }

    /// <summary>Base branch (where the PR merges into). Maps to <c>--base</c>.</summary>
    public string? Base { get; set; }

    /// <summary>Head branch (the PR's source). Maps to <c>--head</c>.</summary>
    public string? Head { get; set; }

    /// <summary>Mark as draft. Maps to <c>--draft</c>.</summary>
    public bool Draft { get; set; }

    /// <summary>Print details without creating. Maps to <c>--dry-run</c>.</summary>
    public bool DryRun { get; set; }

    /// <summary>Disable maintainer edit. Maps to <c>--no-maintainer-edit</c>.</summary>
    public bool NoMaintainerEdit { get; set; }

    /// <summary>Assignees by login. Repeated as <c>--assignee &lt;login&gt;</c>.</summary>
    public List<string> Assignees { get; } = [];

    /// <summary>Reviewers by handle. Repeated as <c>--reviewer &lt;handle&gt;</c>.</summary>
    public List<string> Reviewers { get; } = [];

    /// <summary>Labels by name. Repeated as <c>--label &lt;name&gt;</c>.</summary>
    public List<string> Labels { get; } = [];

    /// <summary>Project titles to add the PR to. Repeated as <c>--project &lt;title&gt;</c>.</summary>
    public List<string> Projects { get; } = [];

    /// <summary>Milestone name. Maps to <c>--milestone</c>.</summary>
    public string? Milestone { get; set; }

    public GhPrCreateSettings SetTitle(string? title) { Title = title; return this; }
    public GhPrCreateSettings SetBody(string? body) { Body = body; return this; }
    public GhPrCreateSettings SetBodyFile(string? path) { BodyFile = path; return this; }
    public GhPrCreateSettings SetFill(bool v = true) { Fill = v; return this; }
    public GhPrCreateSettings SetFillFirst(bool v = true) { FillFirst = v; return this; }
    public GhPrCreateSettings SetFillVerbose(bool v = true) { FillVerbose = v; return this; }
    public GhPrCreateSettings SetBase(string? branch) { Base = branch; return this; }
    public GhPrCreateSettings SetHead(string? branch) { Head = branch; return this; }
    public GhPrCreateSettings SetDraft(bool v = true) { Draft = v; return this; }
    public GhPrCreateSettings SetDryRun(bool v = true) { DryRun = v; return this; }
    public GhPrCreateSettings SetNoMaintainerEdit(bool v = true) { NoMaintainerEdit = v; return this; }
    public GhPrCreateSettings AddAssignee(string login) { Assignees.Add(login); return this; }
    public GhPrCreateSettings AddReviewer(string handle) { Reviewers.Add(handle); return this; }
    public GhPrCreateSettings AddLabel(string label) { Labels.Add(label); return this; }
    public GhPrCreateSettings AddProject(string title) { Projects.Add(title); return this; }
    public GhPrCreateSettings SetMilestone(string? name) { Milestone = name; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "pr";
        yield return "create";
        if (!string.IsNullOrEmpty(Title)) { yield return "--title"; yield return Title!; }
        if (!string.IsNullOrEmpty(Body)) { yield return "--body"; yield return Body!; }
        if (!string.IsNullOrEmpty(BodyFile)) { yield return "--body-file"; yield return BodyFile!; }
        if (Fill) yield return "--fill";
        if (FillFirst) yield return "--fill-first";
        if (FillVerbose) yield return "--fill-verbose";
        if (!string.IsNullOrEmpty(Base)) { yield return "--base"; yield return Base!; }
        if (!string.IsNullOrEmpty(Head)) { yield return "--head"; yield return Head!; }
        if (Draft) yield return "--draft";
        if (DryRun) yield return "--dry-run";
        if (NoMaintainerEdit) yield return "--no-maintainer-edit";
        foreach (var a in Assignees) { yield return "--assignee"; yield return a; }
        foreach (var r in Reviewers) { yield return "--reviewer"; yield return r; }
        foreach (var l in Labels) { yield return "--label"; yield return l; }
        foreach (var p in Projects) { yield return "--project"; yield return p; }
        if (!string.IsNullOrEmpty(Milestone)) { yield return "--milestone"; yield return Milestone!; }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}

/// <summary>State filter for <c>gh pr list</c>.</summary>
public enum GhPrState
{
    Open,
    Closed,
    Merged,
    All,
}

/// <summary>Settings for <c>gh pr list</c>.</summary>
public sealed class GhPrListSettings : GhCommonSettings
{
    /// <summary>State filter. Maps to <c>--state</c>. Default open.</summary>
    public GhPrState? State { get; set; }

    /// <summary>Filter by author. Maps to <c>--author</c>.</summary>
    public string? Author { get; set; }

    /// <summary>Filter by assignee. Maps to <c>--assignee</c>.</summary>
    public string? Assignee { get; set; }

    /// <summary>Filter by base branch. Maps to <c>--base</c>.</summary>
    public string? Base { get; set; }

    /// <summary>Filter by head branch. Maps to <c>--head</c>.</summary>
    public string? Head { get; set; }

    /// <summary>Search query. Maps to <c>--search</c>.</summary>
    public string? Search { get; set; }

    /// <summary>Labels filter. Repeated as <c>--label &lt;name&gt;</c>.</summary>
    public List<string> Labels { get; } = [];

    /// <summary>Max results. Maps to <c>--limit</c>.</summary>
    public int? Limit { get; set; }

    /// <summary>Output as JSON, picking these fields. Repeated as <c>--json &lt;field&gt;</c>... actually a single comma-joined arg.</summary>
    public List<string> JsonFields { get; } = [];

    public GhPrListSettings SetState(GhPrState state) { State = state; return this; }
    public GhPrListSettings SetAuthor(string? author) { Author = author; return this; }
    public GhPrListSettings SetAssignee(string? assignee) { Assignee = assignee; return this; }
    public GhPrListSettings SetBase(string? branch) { Base = branch; return this; }
    public GhPrListSettings SetHead(string? branch) { Head = branch; return this; }
    public GhPrListSettings SetSearch(string? query) { Search = query; return this; }
    public GhPrListSettings AddLabel(string label) { Labels.Add(label); return this; }
    public GhPrListSettings SetLimit(int limit) { Limit = limit; return this; }
    public GhPrListSettings AddJsonField(string field) { JsonFields.Add(field); return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "pr";
        yield return "list";
        if (State is { } st) { yield return "--state"; yield return StateToken(st); }
        if (!string.IsNullOrEmpty(Author)) { yield return "--author"; yield return Author!; }
        if (!string.IsNullOrEmpty(Assignee)) { yield return "--assignee"; yield return Assignee!; }
        if (!string.IsNullOrEmpty(Base)) { yield return "--base"; yield return Base!; }
        if (!string.IsNullOrEmpty(Head)) { yield return "--head"; yield return Head!; }
        if (!string.IsNullOrEmpty(Search)) { yield return "--search"; yield return Search!; }
        foreach (var l in Labels) { yield return "--label"; yield return l; }
        if (Limit is { } lim) { yield return "--limit"; yield return lim.ToString(); }
        if (JsonFields.Count > 0) { yield return "--json"; yield return string.Join(',', JsonFields); }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }

    internal static string StateToken(GhPrState s) => s switch
    {
        GhPrState.Open => "open",
        GhPrState.Closed => "closed",
        GhPrState.Merged => "merged",
        GhPrState.All => "all",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown state."),
    };
}

/// <summary>Settings for <c>gh pr view [number-or-url]</c>.</summary>
public sealed class GhPrViewSettings : GhCommonSettings
{
    /// <summary>PR number, URL, or branch name. Defaults to the PR for the current branch.</summary>
    public string? Selector { get; set; }

    /// <summary>Output as JSON, picking these fields.</summary>
    public List<string> JsonFields { get; } = [];

    /// <summary>Open in web browser instead of CLI. Maps to <c>--web</c>.</summary>
    public bool Web { get; set; }

    /// <summary>Show comments alongside the PR. Maps to <c>--comments</c>.</summary>
    public bool Comments { get; set; }

    public GhPrViewSettings SetSelector(string? selector) { Selector = selector; return this; }
    public GhPrViewSettings AddJsonField(string field) { JsonFields.Add(field); return this; }
    public GhPrViewSettings SetWeb(bool v = true) { Web = v; return this; }
    public GhPrViewSettings SetComments(bool v = true) { Comments = v; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "pr";
        yield return "view";
        if (!string.IsNullOrEmpty(Selector)) yield return Selector!;
        if (JsonFields.Count > 0) { yield return "--json"; yield return string.Join(',', JsonFields); }
        if (Web) yield return "--web";
        if (Comments) yield return "--comments";
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}

/// <summary>Merge strategy for <c>gh pr merge</c>.</summary>
public enum GhPrMergeMethod
{
    /// <summary><c>--merge</c> (default). Standard merge commit.</summary>
    Merge,
    /// <summary><c>--squash</c>. Squash all commits into one.</summary>
    Squash,
    /// <summary><c>--rebase</c>. Rebase and merge.</summary>
    Rebase,
}

/// <summary>Settings for <c>gh pr merge [number-or-url]</c>.</summary>
public sealed class GhPrMergeSettings : GhCommonSettings
{
    /// <summary>PR number, URL, or branch. Defaults to the PR for the current branch.</summary>
    public string? Selector { get; set; }

    /// <summary>Merge strategy.</summary>
    public GhPrMergeMethod? Method { get; set; }

    /// <summary>Auto-merge when status checks pass. Maps to <c>--auto</c>.</summary>
    public bool Auto { get; set; }

    /// <summary>Delete the branch after merge. Maps to <c>--delete-branch</c>.</summary>
    public bool DeleteBranch { get; set; }

    /// <summary>Custom commit body. Maps to <c>--body</c>.</summary>
    public string? Body { get; set; }

    /// <summary>Custom commit subject. Maps to <c>--subject</c>.</summary>
    public string? Subject { get; set; }

    /// <summary>Match the head commit SHA before merging. Maps to <c>--match-head-commit</c>.</summary>
    public string? MatchHeadCommit { get; set; }

    public GhPrMergeSettings SetSelector(string? selector) { Selector = selector; return this; }
    public GhPrMergeSettings SetMethod(GhPrMergeMethod method) { Method = method; return this; }
    public GhPrMergeSettings SetAuto(bool v = true) { Auto = v; return this; }
    public GhPrMergeSettings SetDeleteBranch(bool v = true) { DeleteBranch = v; return this; }
    public GhPrMergeSettings SetBody(string? body) { Body = body; return this; }
    public GhPrMergeSettings SetSubject(string? subject) { Subject = subject; return this; }
    public GhPrMergeSettings SetMatchHeadCommit(string? sha) { MatchHeadCommit = sha; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "pr";
        yield return "merge";
        if (!string.IsNullOrEmpty(Selector)) yield return Selector!;
        if (Method is { } m) yield return MethodFlag(m);
        if (Auto) yield return "--auto";
        if (DeleteBranch) yield return "--delete-branch";
        if (!string.IsNullOrEmpty(Body)) { yield return "--body"; yield return Body!; }
        if (!string.IsNullOrEmpty(Subject)) { yield return "--subject"; yield return Subject!; }
        if (!string.IsNullOrEmpty(MatchHeadCommit)) { yield return "--match-head-commit"; yield return MatchHeadCommit!; }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }

    private static string MethodFlag(GhPrMergeMethod m) => m switch
    {
        GhPrMergeMethod.Merge => "--merge",
        GhPrMergeMethod.Squash => "--squash",
        GhPrMergeMethod.Rebase => "--rebase",
        _ => throw new ArgumentOutOfRangeException(nameof(m), m, "Unknown merge method."),
    };
}
