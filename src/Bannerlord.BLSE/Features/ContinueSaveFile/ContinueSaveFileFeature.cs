using Bannerlord.BLSE.Features.ContinueSaveFile.Patches;

using HarmonyLib;

using System;

using TaleWorlds.MountAndBlade;

namespace Bannerlord.BLSE.Features.ContinueSaveFile;

public static class ContinueSaveFileFeature
{
    public static string Id = FeatureIds.ContinueSaveFileId;

    private static string? _currentSaveFile;
    private static Harmony? _harmony;

    public static void Enable(Harmony harmony)
    {
        _harmony = harmony;
        ModulePatch.OnSaveGameArgParsed += (_, saveFile) => _currentSaveFile = saveFile;
        ModulePatch.Enable(harmony);

        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;
    }

    private static void CurrentDomainOnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        if (_harmony is null) return;

        if (args.LoadedAssembly.GetName().Name == "SandBox")
        {
            SandBoxSubModulePatch.GetSaveGameArg = GetSaveFile;
            SandBoxSubModulePatch.Enable(_harmony);
            InformationManagerPatch.Enable(_harmony);
        }
        
        if (args.LoadedAssembly.GetName().Name == "SandBox.View")
        {
            SandBoxViewSubModulePatch.GetSaveGameArg = GetSaveFile;
            SandBoxViewSubModulePatch.Enable(_harmony);
            InformationManagerPatch.Enable(_harmony);
        }
    }

    private static string? GetSaveFile(GameStartupInfo startupInfo)
    {
        return _currentSaveFile;
    }
}