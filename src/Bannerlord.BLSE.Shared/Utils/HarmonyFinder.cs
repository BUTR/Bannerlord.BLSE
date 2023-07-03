extern alias ModuleManager;

using System.IO;
using System.Reflection;

#if SHARED
using ModuleManager::Bannerlord.ModuleManager;
#endif

namespace Bannerlord.BLSE.Shared.Utils;

public enum HarmonyDiscoveryResult
{
    Discovered,
    ModuleMissing,
    ModuleSubModuleMissing,
    ModuleSubModuleCorrupted,
    ModuleVersionWrong,
    ModuleBinariesMissing,
    ModuleHarmonyMissing,
    UnknownIssue,
}

public static class HarmonyFinder
{
    private static HarmonyDiscoveryResult TryResolveHarmonyAssembliesFile(AssemblyName assemblyName, out string? path)
    {
        path = null;
        var assemblyNameFull = $"{assemblyName.Name}.dll";

        var configName = Path.GetFileName(Directory.GetCurrentDirectory());

        var harmonyModuleFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "Modules", "Bannerlord.Harmony"));
        if (!Directory.Exists(harmonyModuleFolder))
            return HarmonyDiscoveryResult.ModuleMissing;

#if SHARED
        var harmonySubModule = Path.Combine(harmonyModuleFolder, BUTR.Shared.Helpers.ModuleInfoHelper.SubModuleFile);
        if (!File.Exists(harmonySubModule))
            return HarmonyDiscoveryResult.ModuleSubModuleMissing;

        var doc = new System.Xml.XmlDocument();
        doc.Load(File.Exists(harmonySubModule) ? harmonySubModule : string.Empty);
        var harmonyModuleInfo = ModuleInfoExtended.FromXml(doc);
        if (harmonyModuleInfo is null)
            return HarmonyDiscoveryResult.ModuleSubModuleCorrupted;

        if (new ApplicationVersionComparer().Compare(harmonyModuleInfo.Version, new ApplicationVersion(ApplicationVersionType.Release, 2, 2, 2, 0)) < 0)
            return HarmonyDiscoveryResult.ModuleVersionWrong;
#endif

        var harmonyBinFolder = Path.Combine(harmonyModuleFolder, "bin", configName);
        if (!Directory.Exists(harmonyBinFolder))
            return HarmonyDiscoveryResult.ModuleBinariesMissing;

        var assemblyFile = Path.Combine(harmonyBinFolder, assemblyNameFull);
        if (!File.Exists(assemblyFile))
            return HarmonyDiscoveryResult.ModuleHarmonyMissing;

        path = File.Exists(assemblyFile) ? assemblyFile : string.Empty;
        NtfsUnblocker.UnblockFile(path);
        return HarmonyDiscoveryResult.Discovered;
    }
    private static HarmonyDiscoveryResult TryResolveHarmonyAssembliesFileFromSteam(AssemblyName assemblyName, out string? path)
    {
        path = null;
        var assemblyNameFull = $"{assemblyName.Name}.dll";

        var configName = Path.GetFileName(Directory.GetCurrentDirectory());

        var harmonySteamModuleFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "../", "../", "workshop", "content", "261550", "2859188632"));
        if (!Directory.Exists(harmonySteamModuleFolder))
            return HarmonyDiscoveryResult.ModuleMissing;

#if SHARED
        var harmonySteamSubModule = Path.Combine(harmonySteamModuleFolder, BUTR.Shared.Helpers.ModuleInfoHelper.SubModuleFile);
        if (!File.Exists(harmonySteamSubModule))
            return HarmonyDiscoveryResult.ModuleSubModuleMissing;

        var doc = new System.Xml.XmlDocument();
        doc.Load(File.Exists(harmonySteamSubModule) ? harmonySteamSubModule : string.Empty);
        var harmonyModuleInfo = ModuleInfoExtended.FromXml(doc);
        if (harmonyModuleInfo is null)
            return HarmonyDiscoveryResult.ModuleSubModuleCorrupted;

        if (new ApplicationVersionComparer().Compare(harmonyModuleInfo.Version, new ApplicationVersion(ApplicationVersionType.Release, 2, 10, 0, 0)) < 0)
            return HarmonyDiscoveryResult.ModuleVersionWrong;
#endif

        var harmonyBinSteamFolder = Path.Combine(harmonySteamModuleFolder, "bin", configName);
        if (!Directory.Exists(harmonyBinSteamFolder))
            return HarmonyDiscoveryResult.ModuleBinariesMissing;

        var assemblySteamFile = Path.Combine(harmonyBinSteamFolder, assemblyNameFull);
        if (!File.Exists(assemblySteamFile))
            return HarmonyDiscoveryResult.ModuleHarmonyMissing;

        path = File.Exists(assemblySteamFile) ? assemblySteamFile : string.Empty;
        NtfsUnblocker.UnblockFile(path);
        return HarmonyDiscoveryResult.Discovered;
    }


    public static HarmonyDiscoveryResult TryResolveHarmonyAssembliesFileFull(AssemblyName assemblyName, out string? path)
    {
        var configName = Path.GetFileName(Directory.GetCurrentDirectory());
        var checkSteam = configName == "Win64_Shipping_Client";

        var genericDiscoveryResult = TryResolveHarmonyAssembliesFile(assemblyName, out path);
        if (genericDiscoveryResult == HarmonyDiscoveryResult.Discovered)
            return HarmonyDiscoveryResult.Discovered;

        if (checkSteam && TryResolveHarmonyAssembliesFileFromSteam(assemblyName, out path) == HarmonyDiscoveryResult.Discovered)
            return HarmonyDiscoveryResult.Discovered;

        return genericDiscoveryResult;
    }
}