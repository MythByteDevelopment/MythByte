using System.ComponentModel.DataAnnotations;

namespace MythByte.Client.Options;

public class ClientOptions
{
    public const string SectionName = "Client";

    [Range(320, 7680)]
    public required int WindowWidth { get; init; }

    [Range(180, 4320)]
    public required int WindowHeight { get; init; }

    public required bool Fullscreen { get; init; }

    [Range(30, 240)]
    public required int TargetFramesPerSecond { get; init; }

    [Required]
    public required string StartMap { get; init; }
}
