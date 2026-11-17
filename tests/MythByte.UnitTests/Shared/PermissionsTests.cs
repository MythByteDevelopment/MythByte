using MythByte.Shared.Security;

namespace MythByte.UnitTests.Shared;

public class PermissionsTests
{
    [Theory]
    [InlineData(Roles.Player)]
    [InlineData(Roles.Moderator)]
    [InlineData(Roles.Admin)]
    public void Player_AnyRole_IsAllowed(string role)
    {
        var allowed = Permissions.Player.Split(',');

        Assert.Contains(role, allowed);
    }

    [Fact]
    public void Moderator_PlayerRole_IsNotAllowed()
    {
        var allowed = Permissions.Moderator.Split(',');

        Assert.DoesNotContain(Roles.Player, allowed);
    }

    [Fact]
    public void Admin_OnlyAdminRole_IsAllowed()
    {
        var allowed = Permissions.Admin.Split(',');

        Assert.Equal(new[] { Roles.Admin }, allowed);
    }
}
