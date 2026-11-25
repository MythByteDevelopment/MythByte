using LiteNetLib.Utils;
using MythByte.Shared.Network;

namespace MythByte.Networking.Transport;

public static class HandshakeSerializer
{
    public static void Write(NetDataWriter writer, ConnectRequest request)
    {
        writer.Put(request.ProtocolVersion);
        writer.Put(request.ClientVersion, Protocol.ClientVersionMaxLength);
    }

    public static void Write(NetDataWriter writer, ConnectRejected rejected)
    {
        writer.Put((byte)rejected.Reason);
        writer.Put(rejected.Message, Protocol.MessageMaxLength);
    }

    public static byte[] ToBytes(ConnectRejected rejected)
    {
        var writer = new NetDataWriter();
        Write(writer, rejected);
        return writer.CopyData();
    }

    public static ConnectRequest? ReadConnectRequest(NetDataReader reader)
    {
        if (!reader.TryGetInt(out var protocolVersion) || !reader.TryGetString(out var clientVersion))
            return null;

        if (string.IsNullOrWhiteSpace(clientVersion) || clientVersion.Length > Protocol.ClientVersionMaxLength)
            return null;

        return new ConnectRequest(protocolVersion, clientVersion);
    }

    public static ConnectRejected? ReadConnectRejected(NetDataReader reader)
    {
        if (!reader.TryGetByte(out var reason) || !Enum.IsDefined((RejectReason)reason) || !reader.TryGetString(out var message))
            return null;

        return new ConnectRejected((RejectReason)reason, message);
    }
}
