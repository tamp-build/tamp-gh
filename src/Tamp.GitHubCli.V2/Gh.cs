namespace Tamp.GitHubCli.V2;

/// <summary>
/// Sub-facade for <c>gh release</c> verbs.
/// </summary>
public static class GhRelease
{
    /// <summary><c>gh release create &lt;tag&gt; [files...]</c>. Tag is required.</summary>
    public static CommandPlan Create(Tool tool, Action<GhReleaseCreateSettings> configure)
        => Build<GhReleaseCreateSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh release upload &lt;tag&gt; &lt;files&gt;...</c>. Tag and at least one file are required.</summary>
    public static CommandPlan Upload(Tool tool, Action<GhReleaseUploadSettings> configure)
        => Build<GhReleaseUploadSettings>(tool, configure, s => s.BuildArguments());

    private static CommandPlan Build<T>(Tool tool, Action<T> configure, Func<T, IEnumerable<string>> buildArgs)
        where T : GhCommonSettings, new()
    {
        return GhFacade.Plan(tool, configure, buildArgs);
    }

    // ---- Object-init overloads (0.2.0+, TAM-161) ----
    // Tool-bound wrapper variant: `Verb(Tool tool, TSettings settings)` mirrors
    // the fluent body minus the configure invocation. Both authoring styles
    // produce byte-equal CommandPlans. Fluent stays canonical in docs; object-init
    // is offered for consumers who prefer the C# initializer shape.
    //
    //     GhRelease.Create(tool, new() { Tag = "v1.0.0", Title = "Release 1.0.0" });
    //
    // is equivalent to:
    //
    //     GhRelease.Create(tool, s => s.SetTag("v1.0.0").SetTitle("Release 1.0.0"));

    public static CommandPlan Create(Tool tool, GhReleaseCreateSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan Upload(Tool tool, GhReleaseUploadSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());
}

/// <summary>
/// Sub-facade for <c>gh pr</c> verbs.
/// </summary>
public static class GhPr
{
    /// <summary><c>gh pr create</c>.</summary>
    public static CommandPlan Create(Tool tool, Action<GhPrCreateSettings>? configure = null)
        => GhFacade.Plan<GhPrCreateSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh pr list</c>.</summary>
    public static CommandPlan List(Tool tool, Action<GhPrListSettings>? configure = null)
        => GhFacade.Plan<GhPrListSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh pr view [number-or-url]</c>.</summary>
    public static CommandPlan View(Tool tool, Action<GhPrViewSettings>? configure = null)
        => GhFacade.Plan<GhPrViewSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh pr merge [number-or-url]</c>.</summary>
    public static CommandPlan Merge(Tool tool, Action<GhPrMergeSettings>? configure = null)
        => GhFacade.Plan<GhPrMergeSettings>(tool, configure, s => s.BuildArguments());

    // ---- Object-init overloads (0.2.0+, TAM-161) ----

    public static CommandPlan Create(Tool tool, GhPrCreateSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan List(Tool tool, GhPrListSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan View(Tool tool, GhPrViewSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan Merge(Tool tool, GhPrMergeSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());
}

/// <summary>
/// Sub-facade for <c>gh issue</c> verbs.
/// </summary>
public static class GhIssue
{
    /// <summary><c>gh issue create</c>.</summary>
    public static CommandPlan Create(Tool tool, Action<GhIssueCreateSettings>? configure = null)
        => GhFacade.Plan<GhIssueCreateSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh issue list</c>.</summary>
    public static CommandPlan List(Tool tool, Action<GhIssueListSettings>? configure = null)
        => GhFacade.Plan<GhIssueListSettings>(tool, configure, s => s.BuildArguments());

    /// <summary><c>gh issue close &lt;number-or-url&gt;</c>.</summary>
    public static CommandPlan Close(Tool tool, Action<GhIssueCloseSettings> configure)
        => GhFacade.Plan<GhIssueCloseSettings>(tool, configure, s => s.BuildArguments());

    // ---- Object-init overloads (0.2.0+, TAM-161) ----

    public static CommandPlan Create(Tool tool, GhIssueCreateSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan List(Tool tool, GhIssueListSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());

    public static CommandPlan Close(Tool tool, GhIssueCloseSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());
}

/// <summary>
/// Sub-facade for <c>gh api &lt;endpoint&gt;</c> — the escape hatch for any
/// REST or GraphQL call.
/// </summary>
public static class GhApi
{
    /// <summary><c>gh api &lt;endpoint&gt;</c>.</summary>
    public static CommandPlan Request(Tool tool, Action<GhApiSettings> configure)
        => GhFacade.Plan<GhApiSettings>(tool, configure, s => s.BuildArguments());

    // ---- Object-init overloads (0.2.0+, TAM-161) ----

    public static CommandPlan Request(Tool tool, GhApiSettings settings)
        => GhFacade.Plan(tool, settings, s => s.BuildArguments());
}

internal static class GhFacade
{
    internal static CommandPlan Plan<T>(Tool tool, Action<T>? configure, Func<T, IEnumerable<string>> buildArgs)
        where T : GhCommonSettings, new()
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        var s = new T();
        configure?.Invoke(s);
        return BuildPlan(tool, s, buildArgs);
    }

    internal static CommandPlan Plan<T>(Tool tool, T settings, Func<T, IEnumerable<string>> buildArgs)
        where T : GhCommonSettings
    {
        if (tool is null) throw new ArgumentNullException(nameof(tool));
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        return BuildPlan(tool, settings, buildArgs);
    }

    private static CommandPlan BuildPlan<T>(Tool tool, T s, Func<T, IEnumerable<string>> buildArgs)
        where T : GhCommonSettings
    {
        return new CommandPlan
        {
            Executable = tool.Executable.Value,
            Arguments = buildArgs(s).ToList(),
            Environment = s.BuildEnvironment(),
            WorkingDirectory = s.WorkingDirectory ?? tool.WorkingDirectory,
            Secrets = s.BuildSecrets(),
        };
    }
}
