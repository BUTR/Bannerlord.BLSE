using Bannerlord.LauncherManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx.Patches;

internal static class LauncherVMPatch
{
    public static bool Enable(Harmony harmony)
    {
        var res1 = harmony.TryPatch(
            AccessTools2.Method(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart"),
            transpiler: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
        if (!res1) return false;

        var res2 = harmony.TryPatch(
            AccessTools2.Method(typeof(LauncherVM), "GetApplicationVersionOfModule"),
            prefix: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(GetApplicationVersionOfModulePrefix)));
        if (!res2) return false;

        var res3 = harmony.TryPatch(
            AccessTools2.Method(typeof(LauncherVM), "UpdateAndSaveUserModsData"),
            prefix: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(UpdateAndSaveUserModsDataPrefix)));
        if (!res3) return false;

        // Preventing inlining ExecuteConfirmUnverifiedDLLStart
        harmony.TryPatch(
            AccessTools2.Constructor(typeof(LauncherVM), [typeof(UserDataManager), typeof(Action), typeof(Action)]),
            transpiler: AccessTools2.DeclaredMethod(typeof(LauncherVMPatch), nameof(BlankTranspiler)));
        // Preventing inlining ExecuteConfirmUnverifiedDLLStart

        return true;
    }

    // Disable Vanilla's saving
    public static bool UpdateAndSaveUserModsDataPrefix() => false;

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public static bool GetApplicationVersionOfModulePrefix(string id, ref ApplicationVersion __result)
    {
        if (FeatureIds.LauncherFeatures.Contains(id))
        {
            __result = ApplicationVersion.Empty;
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
}