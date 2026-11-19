using MythByte.Client.World;
using MythByte.UnitTests.Shared;

namespace MythByte.UnitTests.Client;

public class LocalPlayerTests
{
    [Fact]
    public void TryStep_FreeTile_MovesOneTile()
    {
        var map = TestMaps.Create(3, 3);
        var player = new LocalPlayer(1, 1);

        var moved = player.TryStep(1, 0, map);

        Assert.True(moved);
        Assert.Equal((2, 1), (player.X, player.Y));
    }

    [Fact]
    public void TryStep_CollisionTile_StaysInPlace()
    {
        var map = TestMaps.Create(3, 3, collision: [0, 15, 0, 0, 0, 0, 0, 0, 0]);
        var player = new LocalPlayer(1, 1);

        var moved = player.TryStep(0, -1, map);

        Assert.False(moved);
        Assert.Equal((1, 1), (player.X, player.Y));
    }

    [Fact]
    public void TryStep_MapEdge_StaysInPlace()
    {
        var map = TestMaps.Create(3, 3);
        var player = new LocalPlayer(0, 0);

        var moved = player.TryStep(-1, 0, map);

        Assert.False(moved);
        Assert.Equal((0, 0), (player.X, player.Y));
    }
}
