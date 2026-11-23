namespace MythByte.GameServer.Connections;

public class ConnectionManager(TimeProvider time) : IConnectionManager
{
    private readonly Dictionary<int, PlayerConnection> _connections = [];
    private readonly SortedSet<int> _freeNetworkIds = [];
    private int _nextNetworkId = 1;

    public int Count => _connections.Count;
    public IReadOnlyCollection<PlayerConnection> All => _connections.Values;

    public PlayerConnection Add(int peerId, string clientVersion)
    {
        if (_connections.ContainsKey(peerId))
            throw new ArgumentException($"Peer {peerId} is already connected.", nameof(peerId));

        var connection = new PlayerConnection(peerId, NextNetworkId(), clientVersion, time.GetUtcNow().UtcDateTime);
        _connections[peerId] = connection;
        return connection;
    }

    public PlayerConnection? Remove(int peerId)
    {
        if (!_connections.Remove(peerId, out var connection))
            return null;

        _freeNetworkIds.Add(connection.NetworkId);
        return connection;
    }

    public PlayerConnection? Find(int peerId) => _connections.GetValueOrDefault(peerId);

    private int NextNetworkId()
    {
        if (_freeNetworkIds.Count == 0)
            return _nextNetworkId++;

        var id = _freeNetworkIds.Min;
        _freeNetworkIds.Remove(id);
        return id;
    }
}
