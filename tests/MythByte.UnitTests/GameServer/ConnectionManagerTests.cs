using MythByte.GameServer.Connections;

namespace MythByte.UnitTests.GameServer;

public class ConnectionManagerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 23, 15, 0, 0, TimeSpan.Zero);

    private readonly ConnectionManager _sut = new(new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void Add_FirstPlayer_GetsNetworkIdOne()
    {
        var connection = _sut.Add(10, "1.0.0");

        Assert.Equal(1, connection.NetworkId);
        Assert.Equal(10, connection.PeerId);
        Assert.Equal("1.0.0", connection.ClientVersion);
        Assert.Equal(Now.UtcDateTime, connection.ConnectedAt);
        Assert.Equal(1, _sut.Count);
    }

    [Fact]
    public void Add_SecondPlayer_GetsNextNetworkId()
    {
        _sut.Add(10, "1.0.0");

        var connection = _sut.Add(11, "1.0.0");

        Assert.Equal(2, connection.NetworkId);
    }

    [Fact]
    public void Add_AfterRemove_ReusesLowestFreeNetworkId()
    {
        _sut.Add(10, "1.0.0");
        _sut.Add(11, "1.0.0");
        _sut.Add(12, "1.0.0");
        _sut.Remove(12);
        _sut.Remove(10);

        var connection = _sut.Add(13, "1.0.0");

        Assert.Equal(1, connection.NetworkId);
    }

    [Fact]
    public void Add_SamePeerTwice_ThrowsArgumentException()
    {
        _sut.Add(10, "1.0.0");

        Assert.Throws<ArgumentException>(() => _sut.Add(10, "1.0.0"));
    }

    [Fact]
    public void Remove_ConnectedPeer_ReturnsConnectionAndLowersCount()
    {
        _sut.Add(10, "1.0.0");

        var removed = _sut.Remove(10);

        Assert.Equal(10, removed?.PeerId);
        Assert.Equal(0, _sut.Count);
        Assert.Null(_sut.Find(10));
    }

    [Fact]
    public void Remove_UnknownPeer_ReturnsNull()
    {
        var removed = _sut.Remove(99);

        Assert.Null(removed);
    }
}
