using Bannerlord.LauncherEx.Extensions;
using Bannerlord.LauncherEx.Mixins;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx.Patches;

internal static class LauncherModsVMPatch
{
    private static readonly AccessTools.FieldRef<LauncherModsVM, UserData>? _userData =
        AccessTools2.FieldRefAccess<LauncherModsVM, UserData>("_userData");

    public static bool Enable(Harmony harmony)
    {
        var res1 = harmony.TryPatch(
            AccessTools2.Method(typeof(LauncherModsVM), "LoadSubModules"),
            prefix: AccessTools2.DeclaredMethod(typeof(LauncherModsVMPatch), nameof(LoadSubModulesPrefix)));

        return true;
    }

    public static bool LoadSubModulesPrefix(LauncherModsVM __instance)
    {
        if (_userData is null)
            return true;

        if (__instance.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is { } mixin)
            mixin.Initialize();

        return false;
    }
}