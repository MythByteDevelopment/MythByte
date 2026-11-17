using System.Reflection;

namespace MythByte.Shared.Versioning;

public static class AppVersion
{
    public static string Of(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
            return assembly.GetName().Version?.ToString(3) ?? "0.0.0";

        var plusIndex = informational.IndexOf('+');
        return plusIndex >= 0 ? informational[..plusIndex] : informational;
    }
}
