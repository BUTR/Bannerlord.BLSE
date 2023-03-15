using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using Mono.Cecil;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;

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
        if (assemblyName.Name is "0Harmony")
            return ResolveHarmonyAssembly(assemblyName);
        if (assemblyName.Name is "Mono.Cecil" or "Mono.Cecil.Mdb" or "Mono.Cecil.Mdb" or "Mono.Cecil.Rocks" or "MonoMod.Utils" or "MonoMod.RuntimeDetour")
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

    private static string? ResolveHarmonyAssembliesFile(AssemblyName assemblyName)
    {
        var assemblyNameFull = $"{assemblyName.Name}.dll";

        var configName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
        var harmonyModuleFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../", ModuleInfoHelper.ModulesFolder, "Bannerlord.Harmony"));
        // TODO: Proper Steam discovery
        var harmonySteamModuleFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "../", "../", "workshop", "content", "261550", "2859188632"));
        if (!Directory.Exists(harmonyModuleFolder) && !Directory.Exists(harmonySteamModuleFolder))
        {
            MessageBoxWrapper.Show("The Harmony module is missing!\nCan't launch with 'Bannerlord.Harmony' module missing!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }

        var harmonySubModule = Path.Combine(harmonyModuleFolder, ModuleInfoHelper.SubModuleFile);
        var harmonySteamSubModule = Path.Combine(harmonySteamModuleFolder, ModuleInfoHelper.SubModuleFile);
        if (!File.Exists(harmonySubModule) && !File.Exists(harmonySteamSubModule))
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nCan't find '{ModuleInfoHelper.SubModuleFile}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }

        var doc = new XmlDocument();
        doc.Load(File.Exists(harmonySubModule) ? harmonySubModule : File.Exists(harmonySteamSubModule) ? harmonySteamSubModule : string.Empty);
        var harmonyModuleInfo = ModuleInfoExtended.FromXml(doc);
        if (harmonyModuleInfo is null)
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nFailed to read '{ModuleInfoHelper.SubModuleFile}'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }
        if (new ApplicationVersionComparer().Compare(harmonyModuleInfo.Version, new ApplicationVersion(ApplicationVersionType.Release, 2, 10, 0, 0)) < 0)
        {
            MessageBoxWrapper.Show("Wrong Harmony module version! At least v2.10.1.x is required!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }
        
        var harmonyBinFolder = Path.Combine(harmonyModuleFolder, "bin", configName);
        var harmonyBinSteamFolder = Path.Combine(harmonySteamModuleFolder, "bin", configName);
        if (!Directory.Exists(harmonyBinFolder) && !Directory.Exists(harmonyBinSteamFolder))
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nCan't find '{Path.Combine("bin", configName)}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }

        var assemblyFile = Path.Combine(harmonyBinFolder, assemblyNameFull);
        var assemblySteamFile = Path.Combine(harmonyBinSteamFolder, assemblyNameFull);
        if (!File.Exists(assemblyFile) && !File.Exists(assemblySteamFile))
        {
            MessageBoxWrapper.Show($"The Harmony module is corrupted!\nCan't find '{assemblyNameFull}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
            Environment.Exit(1);
            return null;
        }
        
        return File.Exists(assemblyFile) ? assemblyFile : File.Exists(assemblySteamFile) ? assemblySteamFile : string.Empty;
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

        var harmonyX = AssemblyDefinition.ReadAssembly(assemblyFile);
        if (harmonyX.Name.Version < new Version(2, 10, 1, 0))
        {
            MessageBoxWrapper.Show("The Harmony module is corrupted!\nWrong 0Harmony.dll version! At least v2.10.1.x is required!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error, 0, 0);
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

        return Assembly.LoadFrom(assemblyFile);
    }

    public static void Disable()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
    }
}