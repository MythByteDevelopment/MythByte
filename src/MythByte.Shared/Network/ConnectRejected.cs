namespace MythByte.Shared.Network;

public sealed record ConnectRejected(RejectReason Reason, string Message);
