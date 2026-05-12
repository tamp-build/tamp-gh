using Xunit;

namespace Tamp.GitHubCli.V2.Tests;

public sealed class GhTests
{
    private static Tool FakeTool() => new(AbsolutePath.Create("/fake/gh"));

    private static int IndexOf(IReadOnlyList<string> args, string value, int start = 0)
    {
        for (var i = start; i < args.Count; i++)
            if (args[i] == value) return i;
        return -1;
    }

    // ================================================================
    // Cross-cutting: every facade rejects null tool
    // ================================================================

    [Fact]
    public void GhRelease_Create_Throws_On_Null_Tool()
        => Assert.Throws<ArgumentNullException>(() => GhRelease.Create(null!, s => s.SetTag("v1")));

    [Fact]
    public void GhRelease_Upload_Throws_On_Null_Tool()
        => Assert.Throws<ArgumentNullException>(() => GhRelease.Upload(null!, s => s.SetTag("v1").AddFile("a")));

    [Fact]
    public void GhPr_Create_Throws_On_Null_Tool()
        => Assert.Throws<ArgumentNullException>(() => GhPr.Create(null!));

    [Fact]
    public void GhIssue_Close_Throws_On_Null_Tool()
        => Assert.Throws<ArgumentNullException>(() => GhIssue.Close(null!, s => s.SetSelector("1")));

    [Fact]
    public void GhApi_Request_Throws_On_Null_Tool()
        => Assert.Throws<ArgumentNullException>(() => GhApi.Request(null!, s => s.SetEndpoint("user")));

    [Fact]
    public void Every_Verb_Uses_Tool_Path_As_Executable()
    {
        // AbsolutePath normalizes per-OS (drive letter on Windows), so compare against
        // the post-normalization value rather than a hardcoded POSIX shape.
        var path = AbsolutePath.Create("/usr/local/bin/gh");
        var tool = new Tool(path);
        Assert.Equal(path.Value, GhRelease.Create(tool, s => s.SetTag("v1")).Executable);
        Assert.Equal(path.Value, GhPr.Create(tool).Executable);
        Assert.Equal(path.Value, GhIssue.List(tool).Executable);
        Assert.Equal(path.Value, GhApi.Request(tool, s => s.SetEndpoint("user")).Executable);
    }

    // ================================================================
    // Token handling — every facade
    // ================================================================

    [Fact]
    public void Token_Is_Set_As_GH_TOKEN_Env_Var_And_Registered_As_Secret()
    {
        var token = new Secret("GitHubToken", "ghp_abc123");
        var plan = GhPr.List(FakeTool(), s => s.SetToken(token));
        Assert.Equal("ghp_abc123", plan.Environment["GH_TOKEN"]);
        Assert.Same(token, Assert.Single(plan.Secrets));
        // The secret value MUST NOT appear in the args list — gh reads
        // GH_TOKEN from env, not flags.
        Assert.DoesNotContain("ghp_abc123", plan.Arguments);
    }

    [Fact]
    public void No_Token_Means_Empty_Secrets_And_No_GH_TOKEN_Env()
    {
        var plan = GhPr.List(FakeTool());
        Assert.Empty(plan.Secrets);
        Assert.False(plan.Environment.ContainsKey("GH_TOKEN"));
    }

    [Fact]
    public void Token_Plus_Custom_Env_Vars_Both_Land_In_Plan()
    {
        var token = new Secret("T", "x");
        var plan = GhPr.List(FakeTool(), s => s
            .SetToken(token)
            .SetEnvironmentVariable("GH_HOST", "github.example.com"));
        Assert.Equal("x", plan.Environment["GH_TOKEN"]);
        Assert.Equal("github.example.com", plan.Environment["GH_HOST"]);
    }

    // ================================================================
    // gh release create
    // ================================================================

    [Fact]
    public void ReleaseCreate_Throws_When_Tag_Missing()
        => Assert.Throws<InvalidOperationException>(() => GhRelease.Create(FakeTool(), _ => { }));

    [Fact]
    public void ReleaseCreate_Tag_Is_Positional_After_Verb()
    {
        var args = GhRelease.Create(FakeTool(), s => s.SetTag("v1.2.3")).Arguments;
        Assert.Equal("release", args[0]);
        Assert.Equal("create", args[1]);
        Assert.Equal("v1.2.3", args[2]);
    }

    [Fact]
    public void ReleaseCreate_Files_Are_Positional_After_Tag()
    {
        var args = GhRelease.Create(FakeTool(), s => s
            .SetTag("v1")
            .AddFile("a.nupkg")
            .AddFile("b.nupkg")).Arguments;
        Assert.Equal("v1", args[2]);
        Assert.Equal("a.nupkg", args[3]);
        Assert.Equal("b.nupkg", args[4]);
    }

    [Fact]
    public void ReleaseCreate_AddFiles_From_AbsolutePath_Sequence_Round_Trips()
    {
        var paths = new[]
        {
            AbsolutePath.Create("/abs/a.nupkg"),
            AbsolutePath.Create("/abs/b.nupkg"),
        };
        var args = GhRelease.Create(FakeTool(), s => s.SetTag("v1").AddFiles(paths)).Arguments;
        Assert.Equal("/abs/a.nupkg", args[3]);
        Assert.Equal("/abs/b.nupkg", args[4]);
    }

    [Fact]
    public void ReleaseCreate_Title_Notes_NotesFile_GenerateNotes_Round_Trip()
    {
        var args = GhRelease.Create(FakeTool(), s => s
            .SetTag("v1")
            .SetTitle("Release v1")
            .SetNotes("Inline notes.")
            .SetNotesFile("RELEASE.md")
            .SetGenerateNotes()).Arguments;
        Assert.Equal("Release v1", args[IndexOf(args, "--title") + 1]);
        Assert.Equal("Inline notes.", args[IndexOf(args, "--notes") + 1]);
        Assert.Equal("RELEASE.md", args[IndexOf(args, "--notes-file") + 1]);
        Assert.Contains("--generate-notes", args);
    }

    [Fact]
    public void ReleaseCreate_Draft_Prerelease_Latest_Round_Trip()
    {
        var args = GhRelease.Create(FakeTool(), s => s
            .SetTag("v1")
            .SetDraft()
            .SetPrerelease()
            .SetLatest(true)).Arguments;
        Assert.Contains("--draft", args);
        Assert.Contains("--prerelease", args);
        Assert.Equal("true", args[IndexOf(args, "--latest") + 1]);
    }

    [Fact]
    public void ReleaseCreate_Latest_False_Emits_False()
    {
        // Explicitly opting-out of "latest" is meaningful for prerelease
        // ladders.
        var args = GhRelease.Create(FakeTool(), s => s.SetTag("v1").SetLatest(false)).Arguments;
        Assert.Equal("false", args[IndexOf(args, "--latest") + 1]);
    }

    [Fact]
    public void ReleaseCreate_Target_DiscussionCategory_VerifyTag_FailOnNoCommits_Round_Trip()
    {
        var args = GhRelease.Create(FakeTool(), s => s
            .SetTag("v1")
            .SetTarget("main")
            .SetDiscussionCategory("Announcements")
            .SetVerifyTag()
            .SetFailOnNoCommits()).Arguments;
        Assert.Equal("main", args[IndexOf(args, "--target") + 1]);
        Assert.Equal("Announcements", args[IndexOf(args, "--discussion-category") + 1]);
        Assert.Contains("--verify-tag", args);
        Assert.Contains("--fail-on-no-commits", args);
    }

    [Fact]
    public void ReleaseCreate_Repo_Round_Trips_Via_Common_Setting()
    {
        var args = GhRelease.Create(FakeTool(), s => s.SetTag("v1").SetRepo("acme/widget")).Arguments;
        Assert.Equal("acme/widget", args[IndexOf(args, "--repo") + 1]);
    }

    // ================================================================
    // gh release upload
    // ================================================================

    [Fact]
    public void ReleaseUpload_Throws_When_Tag_Missing()
        => Assert.Throws<InvalidOperationException>(() => GhRelease.Upload(FakeTool(), s => s.AddFile("a")));

    [Fact]
    public void ReleaseUpload_Throws_When_No_Files()
        => Assert.Throws<InvalidOperationException>(() => GhRelease.Upload(FakeTool(), s => s.SetTag("v1")));

    [Fact]
    public void ReleaseUpload_Verb_Tokens_Tag_Files_Clobber_All_Round_Trip()
    {
        var args = GhRelease.Upload(FakeTool(), s => s
            .SetTag("v1")
            .AddFile("a.nupkg")
            .AddFile("b.nupkg")
            .SetClobber()).Arguments;
        Assert.Equal(["release", "upload", "v1", "a.nupkg", "b.nupkg", "--clobber"], args);
    }

    // ================================================================
    // gh pr create
    // ================================================================

    [Fact]
    public void PrCreate_Bare_Verb_Tokens()
    {
        var args = GhPr.Create(FakeTool()).Arguments;
        Assert.Equal(["pr", "create"], args);
    }

    [Fact]
    public void PrCreate_Title_Body_Base_Head_Round_Trip()
    {
        var args = GhPr.Create(FakeTool(), s => s
            .SetTitle("Add feature X")
            .SetBody("Implements X.")
            .SetBase("main")
            .SetHead("feature-x")).Arguments;
        Assert.Equal("Add feature X", args[IndexOf(args, "--title") + 1]);
        Assert.Equal("Implements X.", args[IndexOf(args, "--body") + 1]);
        Assert.Equal("main", args[IndexOf(args, "--base") + 1]);
        Assert.Equal("feature-x", args[IndexOf(args, "--head") + 1]);
    }

    [Fact]
    public void PrCreate_Fill_Flags_All_Distinct()
    {
        var fill = GhPr.Create(FakeTool(), s => s.SetFill()).Arguments;
        var fillFirst = GhPr.Create(FakeTool(), s => s.SetFillFirst()).Arguments;
        var fillVerbose = GhPr.Create(FakeTool(), s => s.SetFillVerbose()).Arguments;
        Assert.Contains("--fill", fill);
        Assert.DoesNotContain("--fill-first", fill);
        Assert.Contains("--fill-first", fillFirst);
        Assert.DoesNotContain("--fill", fillFirst);
        Assert.Contains("--fill-verbose", fillVerbose);
    }

    [Fact]
    public void PrCreate_Reviewers_Assignees_Labels_Each_Repeat_Their_Flag()
    {
        var args = GhPr.Create(FakeTool(), s => s
            .AddAssignee("alice")
            .AddAssignee("bob")
            .AddReviewer("@team-x")
            .AddReviewer("carol")
            .AddLabel("bug")
            .AddLabel("priority-high")).Arguments;
        Assert.Equal(2, args.Count(a => a == "--assignee"));
        Assert.Equal(2, args.Count(a => a == "--reviewer"));
        Assert.Equal(2, args.Count(a => a == "--label"));
    }

    [Fact]
    public void PrCreate_Draft_DryRun_NoMaintainerEdit_Round_Trip()
    {
        var args = GhPr.Create(FakeTool(), s => s
            .SetDraft()
            .SetDryRun()
            .SetNoMaintainerEdit()).Arguments;
        Assert.Contains("--draft", args);
        Assert.Contains("--dry-run", args);
        Assert.Contains("--no-maintainer-edit", args);
    }

    // ================================================================
    // gh pr list
    // ================================================================

    [Theory]
    [InlineData(GhPrState.Open, "open")]
    [InlineData(GhPrState.Closed, "closed")]
    [InlineData(GhPrState.Merged, "merged")]
    [InlineData(GhPrState.All, "all")]
    public void PrList_State_Maps_To_Lowercase_Token(GhPrState state, string expected)
    {
        var args = GhPr.List(FakeTool(), s => s.SetState(state)).Arguments;
        Assert.Equal(expected, args[IndexOf(args, "--state") + 1]);
    }

    [Fact]
    public void PrList_JsonFields_Get_Comma_Joined_In_One_Arg()
    {
        // gh's --json flag takes a single comma-separated value, not
        // a repeated flag. Verify the wrapper joins.
        var args = GhPr.List(FakeTool(), s => s
            .AddJsonField("number")
            .AddJsonField("title")
            .AddJsonField("author")).Arguments;
        Assert.Single(args, a => a == "--json");
        Assert.Equal("number,title,author", args[IndexOf(args, "--json") + 1]);
    }

    [Fact]
    public void PrList_Limit_Is_Stringified()
    {
        var args = GhPr.List(FakeTool(), s => s.SetLimit(42)).Arguments;
        Assert.Equal("42", args[IndexOf(args, "--limit") + 1]);
    }

    [Fact]
    public void PrList_Author_Assignee_Search_Round_Trip()
    {
        var args = GhPr.List(FakeTool(), s => s
            .SetAuthor("@me")
            .SetAssignee("alice")
            .SetSearch("is:open label:bug")).Arguments;
        Assert.Equal("@me", args[IndexOf(args, "--author") + 1]);
        Assert.Equal("alice", args[IndexOf(args, "--assignee") + 1]);
        Assert.Equal("is:open label:bug", args[IndexOf(args, "--search") + 1]);
    }

    // ================================================================
    // gh pr view
    // ================================================================

    [Fact]
    public void PrView_Selector_Is_Positional_Web_And_Comments_Round_Trip()
    {
        var args = GhPr.View(FakeTool(), s => s.SetSelector("123").SetWeb().SetComments()).Arguments;
        Assert.Equal("pr", args[0]);
        Assert.Equal("view", args[1]);
        Assert.Equal("123", args[2]);
        Assert.Contains("--web", args);
        Assert.Contains("--comments", args);
    }

    // ================================================================
    // gh pr merge
    // ================================================================

    [Theory]
    [InlineData(GhPrMergeMethod.Merge, "--merge")]
    [InlineData(GhPrMergeMethod.Squash, "--squash")]
    [InlineData(GhPrMergeMethod.Rebase, "--rebase")]
    public void PrMerge_Method_Maps_To_Its_Own_Flag(GhPrMergeMethod method, string flag)
    {
        var args = GhPr.Merge(FakeTool(), s => s.SetMethod(method)).Arguments;
        Assert.Contains(flag, args);
    }

    [Fact]
    public void PrMerge_Auto_DeleteBranch_MatchHeadCommit_Round_Trip()
    {
        var args = GhPr.Merge(FakeTool(), s => s
            .SetSelector("42")
            .SetMethod(GhPrMergeMethod.Squash)
            .SetAuto()
            .SetDeleteBranch()
            .SetMatchHeadCommit("abc123")).Arguments;
        Assert.Equal("42", args[2]);
        Assert.Contains("--squash", args);
        Assert.Contains("--auto", args);
        Assert.Contains("--delete-branch", args);
        Assert.Equal("abc123", args[IndexOf(args, "--match-head-commit") + 1]);
    }

    // ================================================================
    // gh issue create
    // ================================================================

    [Fact]
    public void IssueCreate_Title_Body_Labels_Round_Trip()
    {
        var args = GhIssue.Create(FakeTool(), s => s
            .SetTitle("Bug: foo crashes")
            .SetBody("Repro steps...")
            .AddLabel("bug")
            .AddLabel("regression")
            .AddAssignee("alice")).Arguments;
        Assert.Equal("Bug: foo crashes", args[IndexOf(args, "--title") + 1]);
        Assert.Equal(2, args.Count(a => a == "--label"));
    }

    // ================================================================
    // gh issue list
    // ================================================================

    [Theory]
    [InlineData(GhIssueState.Open, "open")]
    [InlineData(GhIssueState.Closed, "closed")]
    [InlineData(GhIssueState.All, "all")]
    public void IssueList_State_Maps_To_Lowercase_Token(GhIssueState state, string expected)
    {
        var args = GhIssue.List(FakeTool(), s => s.SetState(state)).Arguments;
        Assert.Equal(expected, args[IndexOf(args, "--state") + 1]);
    }

    // ================================================================
    // gh issue close
    // ================================================================

    [Fact]
    public void IssueClose_Throws_When_Selector_Missing()
        => Assert.Throws<InvalidOperationException>(() => GhIssue.Close(FakeTool(), _ => { }));

    [Fact]
    public void IssueClose_Selector_Reason_Comment_Round_Trip()
    {
        var args = GhIssue.Close(FakeTool(), s => s
            .SetSelector("42")
            .SetReason("not planned")
            .SetComment("Wontfix.")).Arguments;
        Assert.Equal("42", args[2]);
        Assert.Equal("not planned", args[IndexOf(args, "--reason") + 1]);
        Assert.Equal("Wontfix.", args[IndexOf(args, "--comment") + 1]);
    }

    // ================================================================
    // gh api
    // ================================================================

    [Fact]
    public void ApiRequest_Throws_When_Endpoint_Missing()
        => Assert.Throws<InvalidOperationException>(() => GhApi.Request(FakeTool(), _ => { }));

    [Fact]
    public void ApiRequest_Endpoint_Is_Positional_After_Verb()
    {
        var args = GhApi.Request(FakeTool(), s => s.SetEndpoint("repos/{owner}/{repo}/releases")).Arguments;
        Assert.Equal("api", args[0]);
        Assert.Equal("repos/{owner}/{repo}/releases", args[1]);
    }

    [Theory]
    [InlineData(GhApiMethod.Get, "GET")]
    [InlineData(GhApiMethod.Post, "POST")]
    [InlineData(GhApiMethod.Put, "PUT")]
    [InlineData(GhApiMethod.Patch, "PATCH")]
    [InlineData(GhApiMethod.Delete, "DELETE")]
    [InlineData(GhApiMethod.Head, "HEAD")]
    public void ApiRequest_Method_Maps_To_Uppercase_Token(GhApiMethod method, string expected)
    {
        var args = GhApi.Request(FakeTool(), s => s.SetEndpoint("user").SetMethod(method)).Arguments;
        Assert.Equal(expected, args[IndexOf(args, "--method") + 1]);
    }

    [Fact]
    public void ApiRequest_Fields_RawFields_Headers_Each_Repeat_Their_Flag()
    {
        var args = GhApi.Request(FakeTool(), s => s
            .SetEndpoint("repos/x/y/issues")
            .AddField("title", "Bug")
            .AddField("labels[]", "bug")
            .AddRawField("state", "open")
            .AddHeader("X-GitHub-Api-Version", "2022-11-28")).Arguments;
        Assert.Equal(2, args.Count(a => a == "--field"));
        Assert.Single(args, a => a == "--raw-field");
        Assert.Single(args, a => a == "--header");
        Assert.Equal("title=Bug", args[IndexOf(args, "--field") + 1]);
        Assert.Equal("state=open", args[IndexOf(args, "--raw-field") + 1]);
        Assert.Equal("X-GitHub-Api-Version:2022-11-28", args[IndexOf(args, "--header") + 1]);
    }

    [Fact]
    public void ApiRequest_Jq_Template_Cache_Hostname_Round_Trip()
    {
        var args = GhApi.Request(FakeTool(), s => s
            .SetEndpoint("user")
            .SetJq(".login")
            .SetTemplate("{{.login}}")
            .SetCache("60s")
            .SetHostname("github.example.com")).Arguments;
        Assert.Equal(".login", args[IndexOf(args, "--jq") + 1]);
        Assert.Equal("{{.login}}", args[IndexOf(args, "--template") + 1]);
        Assert.Equal("60s", args[IndexOf(args, "--cache") + 1]);
        Assert.Equal("github.example.com", args[IndexOf(args, "--hostname") + 1]);
    }

    [Fact]
    public void ApiRequest_Paginate_Slurp_IncludeHeaders_Silent_Verbose_Round_Trip()
    {
        var args = GhApi.Request(FakeTool(), s => s
            .SetEndpoint("repos/x/y/issues")
            .SetPaginate()
            .SetSlurp()
            .SetIncludeHeaders()
            .SetSilent()
            .SetVerbose()).Arguments;
        Assert.Contains("--paginate", args);
        Assert.Contains("--slurp", args);
        Assert.Contains("--include", args);
        Assert.Contains("--silent", args);
        Assert.Contains("--verbose", args);
    }

    [Fact]
    public void ApiRequest_Does_NOT_Emit_Repo_Flag()
    {
        // gh api takes the repo via the endpoint placeholders, not via
        // --repo. The wrapper omits --repo for api calls even when set.
        var args = GhApi.Request(FakeTool(), s => s
            .SetEndpoint("user")
            .SetRepo("acme/widget")).Arguments;
        Assert.DoesNotContain("--repo", args);
    }

    [Fact]
    public void ApiRequest_Previews_Each_Repeat_The_Flag()
    {
        var args = GhApi.Request(FakeTool(), s => s
            .SetEndpoint("user")
            .AddPreview("squirrel-girl")
            .AddPreview("hellcat")).Arguments;
        Assert.Equal(2, args.Count(a => a == "--preview"));
    }

    // ================================================================
    // Working directory precedence
    // ================================================================

    [Fact]
    public void WorkingDirectory_From_Settings_Wins_Over_Tool()
    {
        var tool = new Tool(AbsolutePath.Create("/fake/gh"), workingDirectory: "/from-tool");
        var plan = GhPr.List(tool, s => s.SetWorkingDirectory("/from-settings"));
        Assert.Equal("/from-settings", plan.WorkingDirectory);
    }

    [Fact]
    public void WorkingDirectory_Falls_Back_To_Tool_When_Settings_Null()
    {
        var tool = new Tool(AbsolutePath.Create("/fake/gh"), workingDirectory: "/from-tool");
        var plan = GhPr.List(tool);
        Assert.Equal("/from-tool", plan.WorkingDirectory);
    }
}
