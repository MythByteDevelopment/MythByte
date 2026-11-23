using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using MythByte.GameServer;
using MythByte.GameServer.Connections;
using MythByte.GameServer.Options;
using MythByte.Networking.Transport;
using MythByte.Shared.Network;
using MythByte.Shared.Versioning;

namespace MythByte.IntegrationTests;

public class GameServerConnectionTests : IAsyncLifetime
{
    private static readonly string ServerVersion = AppVersion.Of(typeof(GameServerWorker).Assembly);

    private readonly int _port = FreeUdpPort();
    private readonly LiteNetServerTransport _transport = new();
    private readonly ConnectionManager _connections = new(TimeProvider.System);
    private readonly List<LiteNetClientTransport> _clients = [];
    private GameServerWorker _worker = null!;

    public Task InitializeAsync() => StartServerAsync(maxPlayers: 2);

    public async Task DisposeAsync()
    {
        await _worker.StopAsync(CancellationToken.None);
        _worker.Dispose();
        _transport.Dispose();
        foreach (var client in _clients)
            client.Dispose();
    }

    private async Task StartServerAsync(int maxPlayers)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new GameServerOptions
        {
            Port = _port,
            TickRate = 60,
            MaxPlayers = maxPlayers,
            DisconnectTimeoutMilliseconds = 1000
        });
        _worker = new GameServerWorker(_transport, _connections, options, NullLogger<GameServerWorker>.Instance);
        await _worker.StartAsync(CancellationToken.None);
    }

    private static int FreeUdpPort()
    {
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)udp.Client.LocalEndPoint!).Port;
    }

    private static bool WaitUntil(Func<bool> condition, params LiteNetClientTransport[] clients)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            foreach (var client in clients)
                client.Poll();

            if (condition())
                return true;

            Thread.Sleep(10);
        }

        return false;
    }

    private (LiteNetClientTransport Client, ClientEvents Events) Connect(string clientVersion, int protocolVersion = Protocol.Version)
    {
        var client = new LiteNetClientTransport();
        var events = new ClientEvents();
        client.Connected += () => events.Connected = true;
        client.Disconnected += (cause, rejected) =>
        {
            events.Cause = cause;
            events.Rejected = rejected;
        };
        _clients.Add(client);
        client.Connect("127.0.0.1", _port, new ConnectRequest(protocolVersion, clientVersion), 1000);
        return (client, events);
    }

    private sealed class ClientEvents
    {
        public bool Connected { get; set; }
        public DisconnectCause? Cause { get; set; }
        public ConnectRejected? Rejected { get; set; }
    }

    [Fact]
    public void Connect_MatchingVersion_IsAcceptedAndTracked()
    {
        var (client, events) = Connect(ServerVersion);

        var connected = WaitUntil(() => events.Connected && _connections.Count == 1, client);

        Assert.True(connected);
        Assert.Equal(1, _connections.All.Single().NetworkId);
    }

    [Fact]
    public void Connect_OutdatedClient_IsRejectedWithUpdateMessage()
    {
        var (client, events) = Connect("0.0.1-old");

        var rejected = WaitUntil(() => events.Cause is not null, client);

        Assert.True(rejected);
        Assert.Equal(DisconnectCause.Rejected, events.Cause);
        Assert.Equal(RejectReason.OutdatedClient, events.Rejected?.Reason);
        Assert.Equal($"Update your client to v{ServerVersion}.", events.Rejected?.Message);
        Assert.Equal(0, _connections.Count);
    }

    [Fact]
    public void Connect_OtherProtocol_IsRejectedWithUpdateMessage()
    {
        var (client, events) = Connect(ServerVersion, Protocol.Version + 1);

        WaitUntil(() => events.Cause is not null, client);

        Assert.Equal(RejectReason.OutdatedClient, events.Rejected?.Reason);
    }

    [Fact]
    public void Connect_MoreThanMaxPlayers_IsRejectedAsFull()
    {
        var (first, firstEvents) = Connect(ServerVersion);
        var (second, secondEvents) = Connect(ServerVersion);
        WaitUntil(() => firstEvents.Connected && secondEvents.Connected, first, second);
        var (third, thirdEvents) = Connect(ServerVersion);

        var rejected = WaitUntil(() => thirdEvents.Cause is not null, first, second, third);

        Assert.True(rejected);
        Assert.Equal(RejectReason.ServerFull, thirdEvents.Rejected?.Reason);
        Assert.Equal(2, _connections.Count);
    }

    [Fact]
    public void Disconnect_ClientCloses_RemovesPlayer()
    {
        var (client, events) = Connect(ServerVersion);
        WaitUntil(() => events.Connected && _connections.Count == 1, client);

        client.Disconnect();
        var removed = WaitUntil(() => _connections.Count == 0, client);

        Assert.True(removed);
    }

    [Fact]
    public void Timeout_ClientVanishes_RemovesPlayer()
    {
        var listener = new EventBasedNetListener();
        var manager = new NetManager(listener) { DisconnectTimeout = 1000 };
        var connected = false;
        listener.PeerConnectedEvent += _ => connected = true;
        manager.Start();
        var writer = new NetDataWriter();
        HandshakeSerializer.Write(writer, new ConnectRequest(Protocol.Version, ServerVersion));
        manager.Connect("127.0.0.1", _port, writer);
        var stopwatch = Stopwatch.StartNew();
        while (!(connected && _connections.Count == 1) && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            manager.PollEvents();
            Thread.Sleep(10);
        }

        manager.Stop(false);
        var removed = WaitUntil(() => _connections.Count == 0);

        Assert.True(connected);
        Assert.True(removed);
    }

    [Fact]
    public async Task StopAsync_ConnectedClient_GetsShutdownMessage()
    {
        var (client, events) = Connect(ServerVersion);
        WaitUntil(() => events.Connected && _connections.Count == 1, client);

        await _worker.StopAsync(CancellationToken.None);
        var notified = WaitUntil(() => events.Cause is not null, client);

        Assert.True(notified);
        Assert.Equal(RejectReason.ServerShutdown, events.Rejected?.Reason);
    }
}
