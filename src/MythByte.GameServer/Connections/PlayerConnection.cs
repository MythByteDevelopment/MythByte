namespace MythByte.GameServer.Connections;

public sealed record PlayerConnection(int PeerId, int NetworkId, string ClientVersion, DateTime ConnectedAt);
