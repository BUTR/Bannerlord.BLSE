
using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

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

    private static string? ResolveHarmonyAssembliesFile(AssemblyName assemblyName)
    {
        var assemblyNameFull = $"{assemblyName.Name}.dll";
        var configName = Path.GetFileName(Directory.GetCurrentDirectory());

        switch (HarmonyFinder.TryResolveHarmonyAssembliesFileFull(assemblyName, out var path))
        {
            case HarmonyDiscoveryResult.Discovered:
                return path;
            case HarmonyDiscoveryResult.ModuleMissing:
                MessageBoxDialog.Show("""
The Harmony module is missing!
Can't launch with 'Bannerlord.Harmony' module missing!

If the module was installed manually, make sure that the module in installed in 'Modules/Bannerlord.Harmony'!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.ModuleSubModuleMissing:
                MessageBoxDialog.Show($"""
The Harmony module is corrupted!
Can't find '{ModuleInfoHelper.SubModuleFile}' in 'Bannerlord.Harmony'!

If the module was installed manually, try to do a clean reinstall!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.ModuleSubModuleCorrupted:
                MessageBoxDialog.Show($"""
The Harmony module is corrupted!
Failed to read '{ModuleInfoHelper.SubModuleFile}'!

If the module was installed manually, try to do a clean reinstall!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.ModuleVersionWrong:
                MessageBoxDialog.Show("""
The Harmony module is wrong!
At least v2.2.2.x is required!

If the module was installed manually, find and install the latest version!
If Vortex is used, try to reinstall manually the latest version!
If Steam is used, download the latest Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.ModuleBinariesMissing:
                MessageBoxDialog.Show($"""
The Harmony module is corrupted!
Can't find '{Path.Combine("bin", configName)}' in 'Bannerlord.Harmony'!

If the module was installed manually, try to do a clean reinstall!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.ModuleHarmonyMissing:
                MessageBoxDialog.Show($"""
The Harmony module is corrupted!
Can't find '{assemblyNameFull}' in 'Bannerlord.Harmony'!

If the module was installed manually, try to do a clean reinstall!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                Environment.Exit(1);
                return null;
            case HarmonyDiscoveryResult.UnknownIssue:
            default:
                return null;
        }
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs e)
    {
        if (e.Name is null)
            return null;

        var assemblyName = new AssemblyName(e.Name);
        if (assemblyName.Name is "0Harmony")
            return ResolveHarmonyAssembly(assemblyName);
        if (assemblyName.Name is "Mono.Cecil" or "Mono.Cecil.Mdb" or "Mono.Cecil.Mdb" or "Mono.Cecil.Rocks")
            return ResolveHarmonyAssemblies(assemblyName);
        if (assemblyName.Name is "MonoMod.Core" or "MonoMod.Utils" or "MonoMod.RuntimeDetour" or "MonoMod.Backports" or "MonoMod.Iced" or "MonoMod.ILHelpers")
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
        var closestVersion = versions.Where(x => comparer.Compare(x.Item2, gameVersion) <= 0).MaxByOrDefault(x => x.Item2, comparer, out _);
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

    private static Assembly? ResolveHarmonyAssembly(AssemblyName assemblyName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName == assemblyName.FullName)
                return assembly;
        }

        var assemblyFile = ResolveHarmonyAssembliesFile(assemblyName);
        if (string.IsNullOrEmpty(assemblyFile))
            return null;


        if (ReflectionParser.GetAssemblyVersion(assemblyFile) < new Version(2, 2, 2, 0))
        {
            MessageBoxDialog.Show("""
The Harmony module is corrupted!
Wrong 0Harmony.dll version! At least v2.2.2.x is required!

If the module was installed manually, try to do a clean reinstall!
If Vortex is used, try to reinstall manually!
If Steam is used, download the Harmony mod from NexusMods!
Don't forget that on Steam you have the '/Modules' folder in the Game
and the Steam Workshop folder that can conflict with each other!
""", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
            Environment.Exit(1);
            return null;
        }

        return Assembly.LoadFrom(assemblyFile);
    }
    private static Assembly? ResolveHarmonyAssemblies(AssemblyName assemblyName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName == assemblyName.FullName)
                return assembly;
        }

        var assemblyFile = ResolveHarmonyAssembliesFile(assemblyName);
        if (string.IsNullOrEmpty(assemblyFile))
            return null;

        NtfsUnblocker.UnblockFile(assemblyFile!);
        return Assembly.LoadFrom(assemblyFile);
    }

    public static void Disable()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
    }
}