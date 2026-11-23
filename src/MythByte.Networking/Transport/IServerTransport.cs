using MythByte.Shared.Network;

namespace MythByte.Networking.Transport;

public interface IServerTransport : IDisposable
{
    Func<ConnectRequest?, ConnectRejected?>? Approve { get; set; }
    event Action<int, ConnectRequest>? Connected;
    event Action<int, DisconnectCause>? Disconnected;
    bool IsRunning { get; }
    int PendingCount { get; }
    void Start(int port, int disconnectTimeoutMilliseconds);
    void Poll();
    void Disconnect(int peerId, ConnectRejected reason);
    void Stop(ConnectRejected reason);
}
