using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BLSE.Features.AssemblyResolver;

internal static class AssemblyResolverCore
{
    private const string ModulesMarker = "_MODULES_";

    public static Assembly? Resolve(AssemblyName requested, IEnumerable<string> moduleDirectories)
    {
        if (requested.Name is null) return null;

        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Select(x => (Assembly: x, Name: x.GetName()))
            .Where(x => IsSameAssembly(requested, x.Name))
            .OrderByDescending(x => IsRequestedVersion(requested, x.Name))
            .ThenByDescending(x => x.Name.Version)
            .FirstOrDefault();
        if (loaded.Assembly is not null) return loaded.Assembly;

        var file = GetProbingDirectories().Concat(moduleDirectories)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => Path.Combine(x, $"{requested.Name}.dll"))
            .Where(File.Exists)
            .Select(x => (Path: x, Name: TryGetAssemblyName(x)!))
            .Where(x => x.Name is not null && IsSameAssembly(requested, x.Name))
            .OrderByDescending(x => IsRequestedVersion(requested, x.Name))
            .ThenByDescending(x => requested.Version is null || x.Name.Version >= requested.Version)
            .FirstOrDefault();
        return file.Path is not null ? Assembly.LoadFrom(file.Path) : null;
    }

    public static string[]? ParseModuleIds(IEnumerable<string?>? args)
    {
        if (args is null) return null;

        var commandLine = string.Join(" ", args.Where(x => x is not null));
        var start = commandLine.IndexOf($"{ModulesMarker}*", StringComparison.Ordinal);
        if (start < 0) return null;
        start += ModulesMarker.Length + 1;

        var end = commandLine.IndexOf($"*{ModulesMarker}", start - 1, StringComparison.Ordinal);
        if (end < start) return null;

        var ids = commandLine.Substring(start, end - start).Split('*').Where(x => x.Length > 0).ToArray();
        return ids.Length > 0 ? ids : null;
    }

    private static IEnumerable<string> GetProbingDirectories()
    {
        var domain = AppDomain.CurrentDomain;
        yield return domain.BaseDirectory;

        if (domain.RelativeSearchPath is not { } privateBinPaths) yield break;
        foreach (var privateBinPath in privateBinPaths.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            yield return Path.Combine(domain.BaseDirectory, privateBinPath);
    }

    private static AssemblyName? TryGetAssemblyName(string path)
    {
        try
        {
            return AssemblyName.GetAssemblyName(path);
        }
        catch (Exception)
        {
            // Not a managed assembly, or unreadable
            return null;
        }
    }

    /// <summary>The same name and culture. Versions of it are interchangeable here, cultures are not.</summary>
    private static bool IsSameAssembly(AssemblyName requested, AssemblyName definition) =>
        string.Equals(requested.Name, definition.Name, StringComparison.OrdinalIgnoreCase) &&
        (requested.CultureName is null || string.Equals(requested.CultureName, definition.CultureName ?? string.Empty, StringComparison.OrdinalIgnoreCase));

    /// <summary>The requested version and public key token. A field the request leaves unspecified matches anything.</summary>
    private static bool IsRequestedVersion(AssemblyName requested, AssemblyName definition) =>
        (requested.Version is null || requested.Version == definition.Version) &&
        (requested.GetPublicKeyToken() is not { } token || token.SequenceEqual(definition.GetPublicKeyToken() ?? Array.Empty<byte>()));
}
