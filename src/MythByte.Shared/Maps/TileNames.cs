namespace MythByte.Shared.Maps;

public static class TileNames
{
    public const string PropertyPrefix = "tileNames:";
    public const string TileProperty = "name";

    private const char EntrySeparator = ';';
    private const char ValueSeparator = '=';

    public static string PropertyName(string tilesetName) => PropertyPrefix + tilesetName;

    public static string Format(IEnumerable<KeyValuePair<int, string>> names)
    {
        var entries = names.OrderBy(n => n.Key).Select(n =>
        {
            if (string.IsNullOrWhiteSpace(n.Value) || n.Value.Contains(EntrySeparator) || n.Value.Contains(ValueSeparator))
                throw new ArgumentException($"Tile name '{n.Value}' must not be empty or contain '{EntrySeparator}' or '{ValueSeparator}'.", nameof(names));

            return $"{n.Key}{ValueSeparator}{n.Value}";
        });

        return string.Join(EntrySeparator, entries);
    }

    public static Dictionary<int, string> Parse(string value)
    {
        var names = new Dictionary<int, string>();

        foreach (var entry in value.Split(EntrySeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split(ValueSeparator, 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !int.TryParse(parts[0], out var localId) || localId < 0 || parts[1].Length == 0)
                throw new FormatException($"'{entry}' is not a valid tile name entry (expected 'id{ValueSeparator}name').");

            names[localId] = parts[1];
        }

        return names;
    }

    public static Dictionary<string, int> ByName(IReadOnlyDictionary<int, string>? names) =>
        (names ?? new Dictionary<int, string>())
            .OrderBy(n => n.Key)
            .GroupBy(n => n.Value, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);
}
