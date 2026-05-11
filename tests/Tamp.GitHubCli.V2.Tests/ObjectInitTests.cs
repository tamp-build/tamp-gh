using Xunit;

namespace Tamp.GitHubCli.V2.Tests;

// ---- Object-init overloads (TAM-161, 0.2.0+) ----
//
// Every public `Verb(Tool, Action<TSettings>)` wrapper has a matching
// `Verb(Tool, TSettings)` overload. Both authoring styles must produce
// byte-equal CommandPlans. Tests below cover one round-trip (fluent vs
// object-init Arguments) plus a smoke test asserting NotNull for every
// added overload.

public sealed class ObjectInitTests
{
    private static Tool FakeTool() => new(AbsolutePath.Create("/fake/gh"));

    [Fact]
    public void Release_Create_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();

        var fluent = GhRelease.Create(tool, s => s
            .SetTag("v1.2.0")
            .SetTitle("Release 1.2.0")
            .SetNotes("Initial cut")
            .SetDraft(true)
            .SetPrerelease(true)
            .AddFile("./out/Foo.nupkg")
            .SetRepo("tamp-build/tamp-gh"));

        var objectInit = GhRelease.Create(tool, new GhReleaseCreateSettings
        {
            Tag = "v1.2.0",
            Title = "Release 1.2.0",
            Notes = "Initial cut",
            Draft = true,
            Prerelease = true,
            Files = { "./out/Foo.nupkg" },
            Repo = "tamp-build/tamp-gh",
        });

        Assert.Equal(fluent.Executable, objectInit.Executable);
        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Pr_Create_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();

        var fluent = GhPr.Create(tool, s => s
            .SetTitle("feat: thing")
            .SetBody("body")
            .SetBase("main")
            .SetHead("topic/thing")
            .SetDraft(true)
            .AddLabel("enhancement")
            .AddReviewer("octocat"));

        var objectInit = GhPr.Create(tool, new GhPrCreateSettings
        {
            Title = "feat: thing",
            Body = "body",
            Base = "main",
            Head = "topic/thing",
            Draft = true,
            Labels = { "enhancement" },
            Reviewers = { "octocat" },
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void Api_Request_ObjectInit_Emits_Identical_Plan_To_Fluent()
    {
        var tool = FakeTool();

        var fluent = GhApi.Request(tool, s => s
            .SetEndpoint("repos/owner/repo/issues")
            .SetMethod(GhApiMethod.Post)
            .AddField("title", "hello")
            .AddHeader("X-GitHub-Api-Version", "2022-11-28")
            .SetPaginate(true));

        var objectInit = GhApi.Request(tool, new GhApiSettings
        {
            Endpoint = "repos/owner/repo/issues",
            Method = GhApiMethod.Post,
            Fields = { ["title"] = "hello" },
            Headers = { ["X-GitHub-Api-Version"] = "2022-11-28" },
            Paginate = true,
        });

        Assert.Equal(fluent.Arguments, objectInit.Arguments);
    }

    [Fact]
    public void All_ObjectInit_Overloads_Return_NonNull_Plans()
    {
        // Smoke test: each wrapper accepts an object-init settings argument
        // and returns a non-null CommandPlan. One assertion per overload added.
        var tool = FakeTool();

        // GhRelease
        Assert.NotNull(GhRelease.Create(tool, new GhReleaseCreateSettings { Tag = "v1" }));
        Assert.NotNull(GhRelease.Upload(tool, new GhReleaseUploadSettings { Tag = "v1", Files = { "a" } }));

        // GhPr
        Assert.NotNull(GhPr.Create(tool, new GhPrCreateSettings()));
        Assert.NotNull(GhPr.List(tool, new GhPrListSettings()));
        Assert.NotNull(GhPr.View(tool, new GhPrViewSettings()));
        Assert.NotNull(GhPr.Merge(tool, new GhPrMergeSettings()));

        // GhIssue
        Assert.NotNull(GhIssue.Create(tool, new GhIssueCreateSettings()));
        Assert.NotNull(GhIssue.List(tool, new GhIssueListSettings()));
        Assert.NotNull(GhIssue.Close(tool, new GhIssueCloseSettings { Selector = "1" }));

        // GhApi
        Assert.NotNull(GhApi.Request(tool, new GhApiSettings { Endpoint = "user" }));
    }

    [Fact]
    public void ObjectInit_Overloads_Reject_Null_Settings()
    {
        var tool = FakeTool();
        Assert.Throws<ArgumentNullException>(() => GhRelease.Create(tool, (GhReleaseCreateSettings)null!));
        Assert.Throws<ArgumentNullException>(() => GhPr.Create(tool, (GhPrCreateSettings)null!));
        Assert.Throws<ArgumentNullException>(() => GhIssue.Close(tool, (GhIssueCloseSettings)null!));
        Assert.Throws<ArgumentNullException>(() => GhApi.Request(tool, (GhApiSettings)null!));
    }

    [Fact]
    public void ObjectInit_Overloads_Reject_Null_Tool()
    {
        Assert.Throws<ArgumentNullException>(() => GhRelease.Create(null!, new GhReleaseCreateSettings { Tag = "v1" }));
        Assert.Throws<ArgumentNullException>(() => GhPr.Create(null!, new GhPrCreateSettings()));
        Assert.Throws<ArgumentNullException>(() => GhIssue.List(null!, new GhIssueListSettings()));
        Assert.Throws<ArgumentNullException>(() => GhApi.Request(null!, new GhApiSettings { Endpoint = "user" }));
    }
}
