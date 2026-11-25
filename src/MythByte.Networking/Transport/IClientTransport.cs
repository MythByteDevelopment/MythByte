using MythByte.Shared.Network;

namespace MythByte.Networking.Transport;

public interface IClientTransport : IDisposable
{
    event Action? Connected;
    event Action<DisconnectCause, ConnectRejected?>? Disconnected;
    bool IsConnected { get; }
    void Connect(string host, int port, ConnectRequest request, int disconnectTimeoutMilliseconds);
    void Poll();
    void Disconnect();
}
