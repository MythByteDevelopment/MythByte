using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MythByte.GameServer.Connections;
using MythByte.GameServer.Options;
using MythByte.Networking.Transport;
using MythByte.Shared.Network;
using MythByte.Shared.Versioning;

namespace MythByte.GameServer;

public class GameServerWorker(
    IServerTransport transport,
    IConnectionManager connections,
    IOptions<GameServerOptions> options,
    ILogger<GameServerWorker> logger) : BackgroundService
{
    private readonly string _serverVersion = AppVersion.Of(typeof(GameServerWorker).Assembly);

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        transport.Approve = Approve;
        transport.Connected += OnConnected;
        transport.Disconnected += OnDisconnected;
        transport.Start(settings.Port, settings.DisconnectTimeoutMilliseconds);
        logger.LogInformation("MythByte game server v{Version} (protocol {Protocol}) listening on UDP {Port}, {TickRate} Hz, max {MaxPlayers} players",
            _serverVersion, Protocol.Version, settings.Port, settings.TickRate, settings.MaxPlayers);

        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        transport.Stop(new ConnectRejected(RejectReason.ServerShutdown, "The server is shutting down."));
        transport.Connected -= OnConnected;
        transport.Disconnected -= OnDisconnected;
        logger.LogInformation("Game server stopped");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1d / options.Value.TickRate));

        try
        {
            do
            {
                try
                {
                    transport.Poll();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Game server tick failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private ConnectRejected? Approve(ConnectRequest? request)
    {
        var rejection = ConnectionRules.Check(request, _serverVersion, connections.Count + transport.PendingCount, options.Value.MaxPlayers);
        if (rejection is not null)
            logger.LogInformation("Rejected connection ({Reason}): client v{ClientVersion}, protocol {Protocol}",
                rejection.Reason, request?.ClientVersion ?? "?", request?.ProtocolVersion.ToString() ?? "?");

        return rejection;
    }

    private void OnConnected(int peerId, ConnectRequest request)
    {
        var connection = connections.Add(peerId, request.ClientVersion);
        logger.LogInformation("Player connected: network id {NetworkId} ({Count}/{MaxPlayers})", connection.NetworkId, connections.Count, options.Value.MaxPlayers);
    }

    private void OnDisconnected(int peerId, DisconnectCause cause)
    {
        var connection = connections.Remove(peerId);
        if (connection is not null)
            logger.LogInformation("Player disconnected ({Cause}): network id {NetworkId} ({Count}/{MaxPlayers})", cause, connection.NetworkId, connections.Count, options.Value.MaxPlayers);
    }
}
