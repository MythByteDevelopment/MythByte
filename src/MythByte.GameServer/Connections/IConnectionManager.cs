namespace MythByte.GameServer.Connections;

public interface IConnectionManager
{
    int Count { get; }
    IReadOnlyCollection<PlayerConnection> All { get; }
    PlayerConnection Add(int peerId, string clientVersion);
    PlayerConnection? Remove(int peerId);
    PlayerConnection? Find(int peerId);
}
