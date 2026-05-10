namespace Tamp.GitHubCli.V2;

/// <summary>Settings for <c>gh issue create</c>.</summary>
public sealed class GhIssueCreateSettings : GhCommonSettings
{
    /// <summary>Issue title. Maps to <c>--title</c>.</summary>
    public string? Title { get; set; }

    /// <summary>Issue body (inline). Maps to <c>--body</c>.</summary>
    public string? Body { get; set; }

    /// <summary>Path to a body file (use "-" for stdin). Maps to <c>--body-file</c>.</summary>
    public string? BodyFile { get; set; }

    /// <summary>Assignees by login. Repeated as <c>--assignee &lt;login&gt;</c>.</summary>
    public List<string> Assignees { get; } = [];

    /// <summary>Labels. Repeated as <c>--label &lt;name&gt;</c>.</summary>
    public List<string> Labels { get; } = [];

    /// <summary>Project titles. Repeated as <c>--project &lt;title&gt;</c>.</summary>
    public List<string> Projects { get; } = [];

    /// <summary>Milestone name. Maps to <c>--milestone</c>.</summary>
    public string? Milestone { get; set; }

    /// <summary>Issue template name. Maps to <c>--template</c>.</summary>
    public string? Template { get; set; }

    public GhIssueCreateSettings SetTitle(string? title) { Title = title; return this; }
    public GhIssueCreateSettings SetBody(string? body) { Body = body; return this; }
    public GhIssueCreateSettings SetBodyFile(string? path) { BodyFile = path; return this; }
    public GhIssueCreateSettings AddAssignee(string login) { Assignees.Add(login); return this; }
    public GhIssueCreateSettings AddLabel(string label) { Labels.Add(label); return this; }
    public GhIssueCreateSettings AddProject(string title) { Projects.Add(title); return this; }
    public GhIssueCreateSettings SetMilestone(string? name) { Milestone = name; return this; }
    public GhIssueCreateSettings SetTemplate(string? name) { Template = name; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "issue";
        yield return "create";
        if (!string.IsNullOrEmpty(Title)) { yield return "--title"; yield return Title!; }
        if (!string.IsNullOrEmpty(Body)) { yield return "--body"; yield return Body!; }
        if (!string.IsNullOrEmpty(BodyFile)) { yield return "--body-file"; yield return BodyFile!; }
        foreach (var a in Assignees) { yield return "--assignee"; yield return a; }
        foreach (var l in Labels) { yield return "--label"; yield return l; }
        foreach (var p in Projects) { yield return "--project"; yield return p; }
        if (!string.IsNullOrEmpty(Milestone)) { yield return "--milestone"; yield return Milestone!; }
        if (!string.IsNullOrEmpty(Template)) { yield return "--template"; yield return Template!; }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}

/// <summary>State filter for <c>gh issue list</c>.</summary>
public enum GhIssueState
{
    Open,
    Closed,
    All,
}

/// <summary>Settings for <c>gh issue list</c>.</summary>
public sealed class GhIssueListSettings : GhCommonSettings
{
    public GhIssueState? State { get; set; }
    public string? Author { get; set; }
    public string? Assignee { get; set; }
    public string? Search { get; set; }
    public List<string> Labels { get; } = [];
    public string? Milestone { get; set; }
    public int? Limit { get; set; }
    public List<string> JsonFields { get; } = [];

    public GhIssueListSettings SetState(GhIssueState state) { State = state; return this; }
    public GhIssueListSettings SetAuthor(string? author) { Author = author; return this; }
    public GhIssueListSettings SetAssignee(string? assignee) { Assignee = assignee; return this; }
    public GhIssueListSettings SetSearch(string? search) { Search = search; return this; }
    public GhIssueListSettings AddLabel(string label) { Labels.Add(label); return this; }
    public GhIssueListSettings SetMilestone(string? name) { Milestone = name; return this; }
    public GhIssueListSettings SetLimit(int limit) { Limit = limit; return this; }
    public GhIssueListSettings AddJsonField(string field) { JsonFields.Add(field); return this; }

    internal IEnumerable<string> BuildArguments()
    {
        yield return "issue";
        yield return "list";
        if (State is { } st) { yield return "--state"; yield return StateToken(st); }
        if (!string.IsNullOrEmpty(Author)) { yield return "--author"; yield return Author!; }
        if (!string.IsNullOrEmpty(Assignee)) { yield return "--assignee"; yield return Assignee!; }
        if (!string.IsNullOrEmpty(Search)) { yield return "--search"; yield return Search!; }
        foreach (var l in Labels) { yield return "--label"; yield return l; }
        if (!string.IsNullOrEmpty(Milestone)) { yield return "--milestone"; yield return Milestone!; }
        if (Limit is { } lim) { yield return "--limit"; yield return lim.ToString(); }
        if (JsonFields.Count > 0) { yield return "--json"; yield return string.Join(',', JsonFields); }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }

    private static string StateToken(GhIssueState s) => s switch
    {
        GhIssueState.Open => "open",
        GhIssueState.Closed => "closed",
        GhIssueState.All => "all",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown state."),
    };
}

/// <summary>Settings for <c>gh issue close &lt;number-or-url&gt;</c>.</summary>
public sealed class GhIssueCloseSettings : GhCommonSettings
{
    /// <summary>Issue number, URL, or branch. Required.</summary>
    public string? Selector { get; set; }

    /// <summary>Close reason. Maps to <c>--reason</c> (one of: completed, "not planned").</summary>
    public string? Reason { get; set; }

    /// <summary>Closing comment. Maps to <c>--comment</c>.</summary>
    public string? Comment { get; set; }

    public GhIssueCloseSettings SetSelector(string? selector) { Selector = selector; return this; }
    public GhIssueCloseSettings SetReason(string? reason) { Reason = reason; return this; }
    public GhIssueCloseSettings SetComment(string? comment) { Comment = comment; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Selector)) throw new InvalidOperationException("gh issue close: Selector is required.");
        yield return "issue";
        yield return "close";
        yield return Selector!;
        if (!string.IsNullOrEmpty(Reason)) { yield return "--reason"; yield return Reason!; }
        if (!string.IsNullOrEmpty(Comment)) { yield return "--comment"; yield return Comment!; }
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}
