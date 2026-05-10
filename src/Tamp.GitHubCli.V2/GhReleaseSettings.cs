namespace Tamp.GitHubCli.V2;

/// <summary>Settings for <c>gh release create &lt;tag&gt;</c>.</summary>
public sealed class GhReleaseCreateSettings : GhCommonSettings
{
    /// <summary>Tag for the release. Required.</summary>
    public string? Tag { get; set; }

    /// <summary>Files to attach as release assets. Positional after the tag.</summary>
    public List<string> Files { get; } = [];

    /// <summary>Release title. Maps to <c>--title</c>.</summary>
    public string? Title { get; set; }

    /// <summary>Release notes (inline). Maps to <c>--notes</c>.</summary>
    public string? Notes { get; set; }

    /// <summary>Path to a notes file (use "-" for stdin). Maps to <c>--notes-file</c>.</summary>
    public string? NotesFile { get; set; }

    /// <summary>Auto-generate notes from commits. Maps to <c>--generate-notes</c>.</summary>
    public bool GenerateNotes { get; set; }

    /// <summary>Pull notes from the tag's annotation. Maps to <c>--notes-from-tag</c>.</summary>
    public bool NotesFromTag { get; set; }

    /// <summary>Tag to start the auto-generated notes from. Maps to <c>--notes-start-tag</c>.</summary>
    public string? NotesStartTag { get; set; }

    /// <summary>Save as draft instead of publishing. Maps to <c>--draft</c>.</summary>
    public bool Draft { get; set; }

    /// <summary>Mark as prerelease. Maps to <c>--prerelease</c>.</summary>
    public bool Prerelease { get; set; }

    /// <summary>Mark this release as "Latest" (true / false override). Maps to <c>--latest</c>.</summary>
    public bool? Latest { get; set; }

    /// <summary>Target branch or commit SHA. Maps to <c>--target</c>.</summary>
    public string? Target { get; set; }

    /// <summary>Discussion category to start a discussion in. Maps to <c>--discussion-category</c>.</summary>
    public string? DiscussionCategory { get; set; }

    /// <summary>Verify the tag exists on the remote before creating. Maps to <c>--verify-tag</c>.</summary>
    public bool VerifyTag { get; set; }

    /// <summary>Fail if no commits since last release. Maps to <c>--fail-on-no-commits</c>.</summary>
    public bool FailOnNoCommits { get; set; }

    public GhReleaseCreateSettings SetTag(string? tag) { Tag = tag; return this; }
    public GhReleaseCreateSettings AddFile(string path) { Files.Add(path); return this; }
    public GhReleaseCreateSettings AddFiles(IEnumerable<string> paths) { Files.AddRange(paths); return this; }
    public GhReleaseCreateSettings AddFiles(IEnumerable<AbsolutePath> paths) { foreach (var p in paths) Files.Add(p.Value); return this; }
    public GhReleaseCreateSettings SetTitle(string? title) { Title = title; return this; }
    public GhReleaseCreateSettings SetNotes(string? notes) { Notes = notes; return this; }
    public GhReleaseCreateSettings SetNotesFile(string? path) { NotesFile = path; return this; }
    public GhReleaseCreateSettings SetGenerateNotes(bool v = true) { GenerateNotes = v; return this; }
    public GhReleaseCreateSettings SetNotesFromTag(bool v = true) { NotesFromTag = v; return this; }
    public GhReleaseCreateSettings SetNotesStartTag(string? tag) { NotesStartTag = tag; return this; }
    public GhReleaseCreateSettings SetDraft(bool v = true) { Draft = v; return this; }
    public GhReleaseCreateSettings SetPrerelease(bool v = true) { Prerelease = v; return this; }
    public GhReleaseCreateSettings SetLatest(bool v) { Latest = v; return this; }
    public GhReleaseCreateSettings SetTarget(string? target) { Target = target; return this; }
    public GhReleaseCreateSettings SetDiscussionCategory(string? category) { DiscussionCategory = category; return this; }
    public GhReleaseCreateSettings SetVerifyTag(bool v = true) { VerifyTag = v; return this; }
    public GhReleaseCreateSettings SetFailOnNoCommits(bool v = true) { FailOnNoCommits = v; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Tag)) throw new InvalidOperationException("gh release create: Tag is required.");
        yield return "release";
        yield return "create";
        yield return Tag!;
        foreach (var f in Files) yield return f;
        if (!string.IsNullOrEmpty(Title)) { yield return "--title"; yield return Title!; }
        if (!string.IsNullOrEmpty(Notes)) { yield return "--notes"; yield return Notes!; }
        if (!string.IsNullOrEmpty(NotesFile)) { yield return "--notes-file"; yield return NotesFile!; }
        if (GenerateNotes) yield return "--generate-notes";
        if (NotesFromTag) yield return "--notes-from-tag";
        if (!string.IsNullOrEmpty(NotesStartTag)) { yield return "--notes-start-tag"; yield return NotesStartTag!; }
        if (Draft) yield return "--draft";
        if (Prerelease) yield return "--prerelease";
        if (Latest is { } l) { yield return "--latest"; yield return l ? "true" : "false"; }
        if (!string.IsNullOrEmpty(Target)) { yield return "--target"; yield return Target!; }
        if (!string.IsNullOrEmpty(DiscussionCategory)) { yield return "--discussion-category"; yield return DiscussionCategory!; }
        if (VerifyTag) yield return "--verify-tag";
        if (FailOnNoCommits) yield return "--fail-on-no-commits";
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}

/// <summary>Settings for <c>gh release upload &lt;tag&gt; &lt;files&gt;...</c>.</summary>
public sealed class GhReleaseUploadSettings : GhCommonSettings
{
    /// <summary>Existing release tag. Required.</summary>
    public string? Tag { get; set; }

    /// <summary>Files to upload. At least one is required.</summary>
    public List<string> Files { get; } = [];

    /// <summary>Overwrite existing assets of the same name. Maps to <c>--clobber</c>.</summary>
    public bool Clobber { get; set; }

    public GhReleaseUploadSettings SetTag(string? tag) { Tag = tag; return this; }
    public GhReleaseUploadSettings AddFile(string path) { Files.Add(path); return this; }
    public GhReleaseUploadSettings AddFiles(IEnumerable<string> paths) { Files.AddRange(paths); return this; }
    public GhReleaseUploadSettings AddFiles(IEnumerable<AbsolutePath> paths) { foreach (var p in paths) Files.Add(p.Value); return this; }
    public GhReleaseUploadSettings SetClobber(bool v = true) { Clobber = v; return this; }

    internal IEnumerable<string> BuildArguments()
    {
        if (string.IsNullOrEmpty(Tag)) throw new InvalidOperationException("gh release upload: Tag is required.");
        if (Files.Count == 0) throw new InvalidOperationException("gh release upload: at least one file is required.");
        yield return "release";
        yield return "upload";
        yield return Tag!;
        foreach (var f in Files) yield return f;
        if (Clobber) yield return "--clobber";
        if (!string.IsNullOrEmpty(Repo)) { yield return "--repo"; yield return Repo!; }
    }
}
