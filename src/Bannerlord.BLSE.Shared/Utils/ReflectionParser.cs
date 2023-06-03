using Mono.Cecil;

using System;

namespace Bannerlord.BLSE.Shared.Utils;

public static class ReflectionParser
{
    public static Version GetAssemblyVersion(string? path)
    {
        if (path is null) return new Version();
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(path);
        return assemblyDefinition.Name.Version;
    }
}