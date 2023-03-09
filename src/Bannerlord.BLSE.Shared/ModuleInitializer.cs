using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using TaleWorlds.Core;

internal static class ModuleInitializer
{
    private static int _isAttached;

    [ModuleInitializer]
    internal static void Action()
    {
        if (Interlocked.Exchange(ref _isAttached, 1) == 1)
            return;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs e)
    {
        if (e.Name is null)
            return null;

        var assemblyName = new AssemblyName(e.Name);
        if (assemblyName.Name is "0Harmony" or "Mono.Cecil" or "Mono.Cecil.Mdb" or "Mono.Cecil.Mdb" or "Mono.Cecil.Rocks" or "MonoMod.Utils" or "MonoMod.RuntimeDetour")
            return ResolveHarmonyAssemblies(assemblyName);
        if (assemblyName.Name is "Bannerlord.LauncherEx")
            return ResolveLauncherExAssemblies(assemblyName);

        return null;
    }

    private static Assembly? ResolveLauncherExAssemblies(AssemblyName assemblyName)
    {
        var @namespace = "Bannerlord.BLSE.Shared.";
        var resources = typeof(ModuleInitializer).Assembly.GetManifestResourceNames().Select(x => x.Remove(0, @namespace.Length));
        var versions = resources.Where(x => x.StartsWith("Bannerlord.LauncherEx_")).Select(x =>
        {
            var split = x.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            return (@namespace + x, ApplicationVersion.TryParse(split[1], out var v) ? v : ApplicationVersion.Empty);
        }).ToArray();
        var gv = ApplicationVersionHelper.GameVersion() ?? TaleWorlds.Library.ApplicationVersion.Empty;
        var gameVersion = new ApplicationVersion((ApplicationVersionType) gv.ApplicationVersionType, gv.Major, gv.Minor, gv.Revision, gv.ChangeSet);

        string? toLoad = null;

        var exactVersion = versions.FirstOrDefault(x => x.Item2.IsSame(gameVersion));
        if (exactVersion.Item2 != ApplicationVersion.Empty)
            toLoad = exactVersion.Item1;

        var comparer = new ApplicationVersionComparer();
        var closestVersion = versions.Where(x => comparer.Compare(x.Item2, gameVersion) <= 0).MaxBy(x => x.Item2, comparer, out var maxKey);
        if (closestVersion.Item2 != ApplicationVersion.Empty)
            toLoad = closestVersion.Item1;

        if (toLoad is not null)
        {
            using var resourceStream = typeof(ModuleInitializer).Assembly.GetManifestResourceStream(toLoad);
            using var decompressStream = new GZipStream(resourceStream, CompressionMode.Decompress);
            using var ms = new MemoryStream();
            decompressStream.CopyTo(ms);
            return Assembly.Load(ms.ToArray());
        }

        return null;
    }
    private static Assembly? ResolveHarmonyAssemblies(AssemblyName assemblyName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName == assemblyName.FullName)
                return assembly;
        }

        var assemblyNameFull = $"{assemblyName.Name}.dll";

        var configName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
        var harmonyModuleFolder = Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "Modules", "Bannerlord.Harmony");
        if (!Directory.Exists(harmonyModuleFolder))
        {
            MessageBoxWrapper.Show("The Harmony module is missing!\nCan't launch with 'Bannerlord.Harmony' module missing!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }
        var harmonyFolder = Path.Combine(harmonyModuleFolder, "bin", configName);
        if (!Directory.Exists(harmonyFolder))
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nCan't find '{Path.Combine("bin", configName)}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }

        var assemblyFile = Path.Combine(harmonyFolder, assemblyNameFull);
        if (!File.Exists(assemblyFile))
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nCan't find '{assemblyNameFull}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }

        return Assembly.LoadFrom(assemblyFile);
    }

    public static void Disable()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
    }
}