using System.Text.Json;
using System.Text.Json.Serialization;

namespace MythByte.Shared.Maps;

internal sealed record TiledMapJson(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("orientation")] string? Orientation,
    [property: JsonPropertyName("infinite")] bool Infinite,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("tilewidth")] int TileWidth,
    [property: JsonPropertyName("tileheight")] int TileHeight,
    [property: JsonPropertyName("tilesets")] List<TiledTilesetJson>? Tilesets,
    [property: JsonPropertyName("layers")] List<TiledLayerJson>? Layers,
    [property: JsonPropertyName("properties")] List<TiledPropertyJson>? Properties);

internal sealed record TiledTilesetJson(
    [property: JsonPropertyName("firstgid")] int FirstGid,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("tilewidth")] int TileWidth,
    [property: JsonPropertyName("tileheight")] int TileHeight,
    [property: JsonPropertyName("tilecount")] int TileCount,
    [property: JsonPropertyName("columns")] int Columns,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("tiles")] List<TiledTileJson>? Tiles);

internal sealed record TiledTileJson(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("properties")] List<TiledPropertyJson>? Properties);

internal sealed record TiledLayerJson(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("encoding")] string? Encoding,
    [property: JsonPropertyName("data")] List<uint>? Data);

internal sealed record TiledPropertyJson(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("value")] JsonElement Value);

[JsonSerializable(typeof(TiledMapJson))]
[JsonSerializable(typeof(TiledTilesetJson))]
internal sealed partial class TiledJsonContext : JsonSerializerContext;
