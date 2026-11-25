namespace MythByte.Shared.Network;

public enum RejectReason : byte
{
    InvalidRequest = 1,
    OutdatedClient = 2,
    ServerFull = 3,
    ServerShutdown = 4
}
