using LiteNetLib.Utils;
using MythByte.Networking.Transport;
using MythByte.Shared.Network;

namespace MythByte.UnitTests.Networking;

public class HandshakeSerializerTests
{
    private static NetDataReader ReaderFor(NetDataWriter writer) => new(writer.CopyData());

    [Fact]
    public void ReadConnectRequest_WrittenRequest_RoundTrips()
    {
        var writer = new NetDataWriter();
        HandshakeSerializer.Write(writer, new ConnectRequest(Protocol.Version, "1.2.3"));

        var request = HandshakeSerializer.ReadConnectRequest(ReaderFor(writer));

        Assert.Equal(new ConnectRequest(Protocol.Version, "1.2.3"), request);
    }

    [Fact]
    public void ReadConnectRejected_WrittenRejection_RoundTrips()
    {
        var rejected = new ConnectRejected(RejectReason.OutdatedClient, "Update your client to v1.2.3.");

        var read = HandshakeSerializer.ReadConnectRejected(new NetDataReader(HandshakeSerializer.ToBytes(rejected)));

        Assert.Equal(rejected, read);
    }

    [Fact]
    public void ReadConnectRequest_EmptyData_ReturnsNull()
    {
        var request = HandshakeSerializer.ReadConnectRequest(new NetDataReader([]));

        Assert.Null(request);
    }

    [Fact]
    public void ReadConnectRequest_EmptyClientVersion_ReturnsNull()
    {
        var writer = new NetDataWriter();
        writer.Put(Protocol.Version);
        writer.Put(string.Empty);

        var request = HandshakeSerializer.ReadConnectRequest(ReaderFor(writer));

        Assert.Null(request);
    }

    [Fact]
    public void ReadConnectRequest_TooLongClientVersion_ReturnsNull()
    {
        var writer = new NetDataWriter();
        writer.Put(Protocol.Version);
        writer.Put(new string('9', Protocol.ClientVersionMaxLength + 1));

        var request = HandshakeSerializer.ReadConnectRequest(ReaderFor(writer));

        Assert.Null(request);
    }

    [Fact]
    public void ReadConnectRejected_UnknownReason_ReturnsNull()
    {
        var writer = new NetDataWriter();
        writer.Put((byte)200);
        writer.Put("Nope");

        var rejected = HandshakeSerializer.ReadConnectRejected(ReaderFor(writer));

        Assert.Null(rejected);
    }
}
