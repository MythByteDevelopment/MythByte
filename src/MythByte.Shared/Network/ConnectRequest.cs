namespace MythByte.Shared.Network;

public sealed record ConnectRequest(int ProtocolVersion, string ClientVersion);
