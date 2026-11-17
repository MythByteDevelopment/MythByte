using MythByte.Shared.Versioning;

namespace MythByte.UnitTests.Shared;

public class AppVersionTests
{
    [Fact]
    public void Of_SharedAssembly_ReturnsVersionWithoutCommitSuffix()
    {
        var assembly = typeof(AppVersion).Assembly;

        var version = AppVersion.Of(assembly);

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.DoesNotContain("+", version);
    }
}
