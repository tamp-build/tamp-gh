using Xunit;

namespace Tamp.GitHubCli.V2.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void Assembly_Loads_And_Sub_Facades_Are_Reachable()
    {
        Assert.NotNull(typeof(GhRelease));
        Assert.NotNull(typeof(GhPr));
        Assert.NotNull(typeof(GhIssue));
        Assert.NotNull(typeof(GhApi));
    }
}
