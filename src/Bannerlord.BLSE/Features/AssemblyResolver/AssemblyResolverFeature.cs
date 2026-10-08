using Bannerlord.BLSE.Features.AssemblyResolver.Patches;
using Bannerlord.BLSE.Utils;
using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.AssemblyResolver;

public static class AssemblyResolverFeature
{
    private static ResolveEventHandler AssemblyLoaderOnAssemblyResolve =
        AccessTools2.GetDelegate<ResolveEventHandler>(typeof(AssemblyLoader), "OnAssemblyResolve")!;

    public static string Id = FeatureIds.AssemblyResolverId;

    private const string HarmonyModuleId = "Bannerlord.Harmony";

    private static string[]? _gameArgumentsModuleIds;

    public static void Enable(Harmony harmony)
    {
        AssemblyLoader.Initialize();
        AppDomain.CurrentDomain.AssemblyResolve -= AssemblyLoaderOnAssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        ProgramPatch.OnMain += args => _gameArgumentsModuleIds = AssemblyResolverCore.ParseModuleIds(args);
        ProgramPatch.Enable(harmony);
    }

    private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        if (args.Name is null) return null;

        try
        {
            var name = new AssemblyName(args.Name);
            if (name.Name is null) return null;

            // The directories are enumerated lazily, so a request answered by a loaded assembly doesn't touch the modules
            return AssemblyResolverCore.Resolve(name, GetModuleDirectories());
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// The bin directories of the enabled modules, in load order. Within the same match quality the first wins.
    /// </summary>
    private static IEnumerable<string> GetModuleDirectories()
    {
        var configName = Path.GetFileName(Directory.GetCurrentDirectory());

        // Without a module list, which is the case in the launcher, no module is enabled yet. Nothing may be
        // loaded from Modules then, except Harmony, which BLSE itself needs. Whatever is loaded in the launcher
        // stays loaded in the game when both run in the same process.
        var moduleIds = GetEnabledModuleIds() ?? [HarmonyModuleId];

        var modules = ModuleInfoHelper.GetModules().ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var moduleId in moduleIds)
        {
            if (modules.TryGetValue(moduleId, out var module))
                yield return Path.Combine(module.Path, "bin", configName);
        }
    }

    /// <summary>
    /// The engine has the module list once it's up. Before that, the arguments the game was started with have it.
    /// Null in the launcher, where nothing is enabled yet.
    /// </summary>
    private static string[]? GetEnabledModuleIds()
    {
        var engineModuleIds = GameUtils.GetModulesNames()?.Where(x => x.Length > 0).ToArray();
        return engineModuleIds is { Length: > 0 } ? engineModuleIds : _gameArgumentsModuleIds;
    }
}
