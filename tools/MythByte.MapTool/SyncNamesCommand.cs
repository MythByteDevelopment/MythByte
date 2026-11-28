using MythByte.Shared.Maps;

namespace MythByte.MapTool;

public class SyncNamesCommand
{
    public const string Name = "sync-names";
    public const string RewriteIdsOption = "--rewrite-ids";

    private const string MapPattern = "*.tmj";

    public int Run(string[] args, TextWriter output)
    {
        if (args.Length == 0 || args[0] != Name)
            return Usage(output);

        var rewriteIds = args.Contains(RewriteIdsOption);
        var paths = args.Skip(1).Where(a => a != RewriteIdsOption).ToList();
        if (paths.Count == 0)
            return Usage(output);

        var maps = paths.SelectMany(MapsIn).Distinct().ToList();
        if (maps.Count == 0)
        {
            output.WriteLine("No .tmj maps found.");
            return 1;
        }

        var failed = false;
        foreach (var map in maps)
        {
            try
            {
                var result = TileNameSync.Sync(map, rewriteIds);
                output.WriteLine(Describe(result, rewriteIds));
                failed |= result.IdsOutdated && !rewriteIds;
            }
            catch (Exception exception) when (exception is InvalidDataException or FileNotFoundException or ArgumentException)
            {
                output.WriteLine($"{map}: {exception.Message}");
                failed = true;
            }
        }

        return failed ? 1 : 0;
    }

    private static IEnumerable<string> MapsIn(string path)
    {
        if (Directory.Exists(path))
            return Directory.EnumerateFiles(path, MapPattern, SearchOption.AllDirectories).Select(Path.GetFullPath);

        return [Path.GetFullPath(path)];
    }

    private static string Describe(TileNameSyncResult result, bool rewriteIds) => result switch
    {
        { IdsOutdated: true } when !rewriteIds => $"{result.Path}: tile ids do not match the current tilesets; run again with {RewriteIdsOption} to rewrite them.",
        { IdsOutdated: true } => $"{result.Path}: tile ids rewritten to the current tileset order and tile names updated.",
        { Changed: true } => $"{result.Path}: tile names updated.",
        _ => $"{result.Path}: up to date."
    };

    private static int Usage(TextWriter output)
    {
        output.WriteLine($"Usage: dotnet run --project tools/MythByte.MapTool -- {Name} [{RewriteIdsOption}] <map.tmj or folder>...");
        return 1;
    }
}
