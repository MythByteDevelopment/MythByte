using LiteNetLib;
using LiteNetLib.Utils;
using MythByte.Shared.Network;

namespace MythByte.Networking.Transport;

public sealed class LiteNetServerTransport : IServerTransport
{
    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _manager;
    private readonly Dictionary<int, NetPeer> _peers = [];
    private readonly Dictionary<int, ConnectRequest> _pending = [];

    public LiteNetServerTransport()
    {
        _manager = new NetManager(_listener) { AutoRecycle = true };
        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;
    }

    public Func<ConnectRequest?, ConnectRejected?>? Approve { get; set; }
    public event Action<int, ConnectRequest>? Connected;
    public event Action<int, DisconnectCause>? Disconnected;
    public bool IsRunning => _manager.IsRunning;
    public int PendingCount => _pending.Count;

    public void Start(int port, int disconnectTimeoutMilliseconds)
    {
        _manager.DisconnectTimeout = disconnectTimeoutMilliseconds;
        if (!_manager.Start(port))
            throw new InvalidOperationException($"The game server could not listen on UDP port {port}.");
    }

    public void Poll() => _manager.PollEvents();

    public void Disconnect(int peerId, ConnectRejected reason)
    {
        if (_peers.TryGetValue(peerId, out var peer))
            peer.Disconnect(HandshakeSerializer.ToBytes(reason));
    }

    public void Stop(ConnectRejected reason)
    {
        if (!_manager.IsRunning)
            return;

        var data = HandshakeSerializer.ToBytes(reason);
        _manager.DisconnectAll(data, 0, data.Length);
        _manager.PollEvents();
        _manager.Stop();
        _peers.Clear();
        _pending.Clear();
    }

    public void Dispose()
    {
        if (_manager.IsRunning)
            _manager.Stop();
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        var connect = HandshakeSerializer.ReadConnectRequest(request.Data);
        var rejection = Approve?.Invoke(connect) ?? new ConnectRejected(RejectReason.ServerShutdown, "The server is not accepting players.");

        if (rejection is not null || connect is null)
        {
            request.Reject(HandshakeSerializer.ToBytes(rejection ?? new ConnectRejected(RejectReason.InvalidRequest, "Invalid connect request.")));
            return;
        }

        var peer = request.Accept();
        _pending[peer.Id] = connect;
    }

    private void OnPeerConnected(NetPeer peer)
    {
        if (!_pending.Remove(peer.Id, out var connect))
            return;

        _peers[peer.Id] = peer;
        Connected?.Invoke(peer.Id, connect);
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        _pending.Remove(peer.Id);
        if (!_peers.Remove(peer.Id))
            return;

        Disconnected?.Invoke(peer.Id, info.Reason switch
        {
            DisconnectReason.Timeout => DisconnectCause.Timeout,
            DisconnectReason.DisconnectPeerCalled => DisconnectCause.Kicked,
            _ => DisconnectCause.Closed
        });
    }
}
