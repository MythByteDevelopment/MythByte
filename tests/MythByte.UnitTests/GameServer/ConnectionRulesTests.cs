using MythByte.GameServer.Connections;
using MythByte.Shared.Network;

namespace MythByte.UnitTests.GameServer;

public class ConnectionRulesTests
{
    private const string ServerVersion = "1.2.3";

    [Fact]
    public void Check_MatchingClientWithRoom_Accepts()
    {
        var rejection = ConnectionRules.Check(new ConnectRequest(Protocol.Version, ServerVersion), ServerVersion, 3, 50);

        Assert.Null(rejection);
    }

    [Fact]
    public void Check_MissingRequest_RejectsAsInvalid()
    {
        var rejection = ConnectionRules.Check(null, ServerVersion, 0, 50);

        Assert.Equal(RejectReason.InvalidRequest, rejection?.Reason);
    }

    [Theory]
    [InlineData(Protocol.Version + 1, ServerVersion)]
    [InlineData(Protocol.Version, "1.2.2")]
    [InlineData(Protocol.Version, "1.3.0")]
    public void Check_OtherProtocolOrVersion_AsksToUpdate(int protocolVersion, string clientVersion)
    {
        var rejection = ConnectionRules.Check(new ConnectRequest(protocolVersion, clientVersion), ServerVersion, 0, 50);

        Assert.Equal(RejectReason.OutdatedClient, rejection?.Reason);
        Assert.Equal("Update your client to v1.2.3.", rejection?.Message);
    }

    [Fact]
    public void Check_ServerExactlyFull_RejectsAsFull()
    {
        var rejection = ConnectionRules.Check(new ConnectRequest(Protocol.Version, ServerVersion), ServerVersion, 50, 50);

        Assert.Equal(RejectReason.ServerFull, rejection?.Reason);
    }

    [Fact]
    public void Check_OneSlotLeft_Accepts()
    {
        var rejection = ConnectionRules.Check(new ConnectRequest(Protocol.Version, ServerVersion), ServerVersion, 49, 50);

        Assert.Null(rejection);
    }

    [Fact]
    public void Check_OutdatedAndFull_AsksToUpdateFirst()
    {
        var rejection = ConnectionRules.Check(new ConnectRequest(Protocol.Version, "0.9.0"), ServerVersion, 50, 50);

        Assert.Equal(RejectReason.OutdatedClient, rejection?.Reason);
    }
}
