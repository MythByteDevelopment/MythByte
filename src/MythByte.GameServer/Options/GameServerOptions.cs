using System.ComponentModel.DataAnnotations;

namespace MythByte.GameServer.Options;

public class GameServerOptions
{
    public const string SectionName = "GameServer";

    [Range(1, 65535)]
    public required int Port { get; init; }

    [Range(1, 120)]
    public required int TickRate { get; init; }

    [Range(1, 1000)]
    public required int MaxPlayers { get; init; }

    [Range(1000, 120000)]
    public required int DisconnectTimeoutMilliseconds { get; init; }
}
