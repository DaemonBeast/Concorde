using System.Reflection;

namespace Concorde.Utilities;

public static class DotnetUtilities
{
    public const string DefaultVersion = "1.0.0";
    
    public static string Version =>
        _version ??=
            typeof(DotnetUtilities)
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? DefaultVersion;
    
    private static string? _version;
}