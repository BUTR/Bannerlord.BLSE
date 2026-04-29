
using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// ReSharper disable once CheckNamespace
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
        var moduleAssembly = typeof(ModuleInitializer).Assembly;
        var @namespace = "Bannerlord.BLSE.Shared.";
        var resources = moduleAssembly.GetManifestResourceNames().Select(x => x.Remove(0, @namespace.Length));
        var versions = resources.Where(x => x.StartsWith("Bannerlord.LauncherEx_")).Select(x =>
        {
            var split = x.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            var versionPart = split[1];
            var dllIdx = versionPart.IndexOf(".dll", StringComparison.OrdinalIgnoreCase);
            if (dllIdx > 0) versionPart = versionPart.Substring(0, dllIdx);
            return (@namespace + x, ApplicationVersion.TryParse(versionPart, out var v) ? v : ApplicationVersion.Empty);
        }).ToArray();
        var gv = ApplicationVersionHelper.GameVersion() ?? TaleWorlds.Library.ApplicationVersion.Empty;
        var gameVersion = new ApplicationVersion((ApplicationVersionType) gv.ApplicationVersionType, gv.Major, gv.Minor, gv.Revision, gv.ChangeSet);

        var comparer = new ApplicationVersionComparer();
        var orderedCandidates = versions
            .OrderByDescending(x => x.Item2.IsSame(gameVersion))
            .ThenBy(x => comparer.Compare(x.Item2, gameVersion) > 0 ? 1 : 0)
            .ThenByDescending(x => x.Item2, comparer)
            .Select(x => x.Item1)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        var failures = new List<(string Resource, Exception Exception)>();
        foreach (var candidate in orderedCandidates)
        {
            try
            {
                using var resourceStream = moduleAssembly.GetManifestResourceStream(candidate);
                if (resourceStream is null) continue;
                using var decompressStream = new GZipStream(resourceStream, CompressionMode.Decompress);
                using var ms = new MemoryStream();
                decompressStream.CopyTo(ms);
                return Assembly.Load(ms.ToArray());
            }
            catch (Exception ex)
            {
                failures.Add((candidate, ex));
            }
        }

        if (failures.Count > 0)
            ReportCorruption(moduleAssembly, failures);

        return null;
    }

    private static void ReportCorruption(Assembly moduleAssembly, List<(string Resource, Exception Exception)> failures)
    {
        var sb = new StringBuilder();
        sb.AppendLine("'Bannerlord.BLSE.Shared.dll' could not decompress its embedded LauncherEx assembly.");
        sb.AppendLine("This usually means the file on disk does not match the official release - typically caused by a partial/failed extraction or by an antivirus rewriting the file.");
        sb.AppendLine();

        var dllPath = "<unknown>";
        var dllSha = "<n/a>";
        long dllSize = 0;
        try
        {
            dllPath = moduleAssembly.Location;
            if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
            {
                dllSize = new FileInfo(dllPath).Length;
                using var sha = SHA256.Create();
                using var fs = File.OpenRead(dllPath);
                dllSha = BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "");
            }
        }
        catch { /* best-effort diagnostic */ }

        sb.AppendLine($"DLL Path:     {dllPath}");
        sb.AppendLine($"DLL Size:     {dllSize:N0} bytes");
        sb.AppendLine($"DLL SHA-256:  {dllSha}");
        sb.AppendLine();

        foreach (var (resource, exception) in failures)
        {
            var resSha = "<n/a>";
            long resSize = 0;
            var firstBytes = "<n/a>";
            try
            {
                using var resourceStream = moduleAssembly.GetManifestResourceStream(resource);
                if (resourceStream is not null)
                {
                    using var ms = new MemoryStream();
                    resourceStream.CopyTo(ms);
                    var bytes = ms.ToArray();
                    resSize = bytes.LongLength;
                    using var sha = SHA256.Create();
                    resSha = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
                    var head = bytes.Length < 16 ? bytes.Length : 16;
                    firstBytes = BitConverter.ToString(bytes, 0, head);
                }
            }
            catch { /* best-effort diagnostic */ }
            sb.AppendLine($"Resource:     {resource}");
            sb.AppendLine($"  Size:       {resSize:N0} bytes");
            sb.AppendLine($"  SHA-256:    {resSha}");
            sb.AppendLine($"  First 16B:  {firstBytes}");
            sb.AppendLine($"  Error:      {exception.GetType().Name}: {exception.Message}");
            sb.AppendLine();
        }

        sb.AppendLine("Compare DLL SHA-256 against the value published with the BLSE release.");
        sb.AppendLine("If they differ, reinstall BLSE manually with 7-Zip into your game's bin folder.");

        MessageBoxDialog.Show(sb.ToString(), "BLSE: Bannerlord.BLSE.Shared.dll integrity error", MessageBoxButtons.Ok, MessageBoxIcon.Error);
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