using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Runtime.CompilerServices;

using Module = TaleWorlds.MountAndBlade.Module;

namespace Bannerlord.BLSE.Features.Interceptor.Patches;

internal static class ModulePatch
{
    public static event Action? OnInitializeSubModulesPrefix;
    public static event Action? OnLoadSubModulesPostfix;

    private static Harmony? _harmony;

    public static bool Enable(Harmony harmony)
    {
        _harmony = harmony;

        var res1 = harmony.TryPatch(
            AccessTools2.Method(typeof(Module), "LoadSubModules"),
            postfix: AccessTools2.Method(typeof(ModulePatch), nameof(LoadSubModulesPostfix)));
        if (!res1) return false;

        var res2 = harmony.TryPatch(
            AccessTools2.Method(typeof(Module), "InitializeSubModules") ??
            AccessTools2.Method(typeof(Module), "InitializeSubModuleBases"),
            prefix: AccessTools2.Method(typeof(ModulePatch), nameof(InitializeSubModulesPrefix)));
        if (!res2) return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void InitializeSubModulesPrefix()
    {
        OnInitializeSubModulesPrefix?.Invoke();

        _harmony?.Unpatch(
            AccessTools2.Method(typeof(Module), "InitializeSubModules") ??
            AccessTools2.Method(typeof(Module), "InitializeSubModuleBases"),
            AccessTools2.Method(typeof(ModulePatch), nameof(InitializeSubModulesPrefix)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LoadSubModulesPostfix()
    {
        OnLoadSubModulesPostfix?.Invoke();

        _harmony?.Unpatch(
            AccessTools2.Method(typeof(Module), "LoadSubModules"),
            AccessTools2.Method(typeof(ModulePatch), nameof(LoadSubModulesPostfix)));
    }
}