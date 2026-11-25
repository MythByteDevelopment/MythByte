using LiteNetLib;
using LiteNetLib.Utils;
using MythByte.Shared.Network;

namespace MythByte.Networking.Transport;

public sealed class LiteNetClientTransport : IClientTransport
{
    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _manager;
    private NetPeer? _server;

    public LiteNetClientTransport()
    {
        _manager = new NetManager(_listener) { AutoRecycle = true };
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;
    }

    public event Action? Connected;
    public event Action<DisconnectCause, ConnectRejected?>? Disconnected;
    public bool IsConnected => _server?.ConnectionState == ConnectionState.Connected;

    public void Connect(string host, int port, ConnectRequest request, int disconnectTimeoutMilliseconds)
    {
        if (!_manager.IsRunning)
        {
            _manager.DisconnectTimeout = disconnectTimeoutMilliseconds;
            _manager.Start();
        }

        var writer = new NetDataWriter();
        HandshakeSerializer.Write(writer, request);
        _server = _manager.Connect(host, port, writer);
    }

    public void Poll() => _manager.PollEvents();

    public void Disconnect()
    {
        _server?.Disconnect();
        _manager.PollEvents();
    }

    public void Dispose()
    {
        if (_manager.IsRunning)
            _manager.Stop();
    }

    private void OnPeerConnected(NetPeer peer) => Connected?.Invoke();

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        _server = null;
        var rejected = info.AdditionalData is { AvailableBytes: > 0 } ? HandshakeSerializer.ReadConnectRejected(info.AdditionalData) : null;

        var cause = info.Reason switch
        {
            DisconnectReason.ConnectionRejected => DisconnectCause.Rejected,
            DisconnectReason.Timeout => DisconnectCause.Timeout,
            DisconnectReason.ConnectionFailed or DisconnectReason.HostUnreachable or DisconnectReason.NetworkUnreachable or DisconnectReason.UnknownHost => DisconnectCause.ConnectionFailed,
            DisconnectReason.RemoteConnectionClose when rejected is not null => DisconnectCause.Kicked,
            _ => DisconnectCause.Closed
        };

        Disconnected?.Invoke(cause, rejected);
    }
}
