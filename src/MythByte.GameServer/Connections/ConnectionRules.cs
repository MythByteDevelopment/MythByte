using MythByte.Shared.Network;

namespace MythByte.GameServer.Connections;

public static class ConnectionRules
{
    public static ConnectRejected? Check(ConnectRequest? request, string serverVersion, int playerCount, int maxPlayers)
    {
        if (request is null)
            return new ConnectRejected(RejectReason.InvalidRequest, "Invalid connect request.");

        if (request.ProtocolVersion != Protocol.Version || request.ClientVersion != serverVersion)
            return new ConnectRejected(RejectReason.OutdatedClient, $"Update your client to v{serverVersion}.");

        if (playerCount >= maxPlayers)
            return new ConnectRejected(RejectReason.ServerFull, $"The server is full ({maxPlayers} players). Try again later.");

        return null;
    }
}
