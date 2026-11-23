namespace MythByte.Shared.Maps;

public static class MapLayers
{
    public const string Ground = "Ground";
    public const string Decoration = "Decoration";
    public const string Collision = "Collision";
    public const string Encounter = "Encounter";

    public static readonly IReadOnlyList<string> Drawn = [Ground, Decoration];
}
