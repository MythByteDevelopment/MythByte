# Networking

MythByte uses UDP with [LiteNetLib](https://github.com/RevenantX/LiteNetLib). The game server listens on port 7780; all values come from `GameServerOptions`.

## Protocol version
`Protocol.Version` in `MythByte.Shared.Network` is the protocol version (currently `1`). Raise it whenever a packet changes. The client and server must also have the same app version (`AppVersion`), so a client from another release is always rejected.

## Connect handshake
The client sends its connect data inside LiteNetLib's connection request; the server answers by accepting or rejecting it. Strings are LiteNetLib strings (UTF-8 with a length prefix).

**ConnectRequest** (client to server)

| Field | Type | Limit |
|---|---|---|
| ProtocolVersion | int | must equal `Protocol.Version` |
| ClientVersion | string | 1 to 32 characters, must equal the server version |

**ConnectRejected** (server to client, as reject or disconnect data)

| Field | Type | Values |
|---|---|---|
| Reason | byte | 1 InvalidRequest, 2 OutdatedClient, 3 ServerFull, 4 ServerShutdown |
| Message | string | Up to 200 characters, shown to the player |

The server checks in this order:

| Check | Result |
|---|---|
| Missing or malformed data | InvalidRequest, "Invalid connect request." |
| Other protocol or client version | OutdatedClient, "Update your client to vX.Y.Z." |
| Players (connected plus pending) at `MaxPlayers` | ServerFull, "The server is full (N players). Try again later." |

When the server stops, every connected client gets a ServerShutdown message.

## Connections
- Each accepted player gets a small network id (the lowest free number from 1). Database Guids never go over the wire.
- A client that stops answering is removed after `DisconnectTimeoutMilliseconds`.
- LiteNetLib events are only handled when the simulation loop polls, so connection state is only touched on the simulation thread.

Game packets (movement, battles, trades) follow in US11.
