using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Patches
{
    internal static class LauncherConfirmStartVMPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(LauncherConfirmStartVM), "EnableWith"),
                prefix: AccessTools2.DeclaredMethod(typeof(LauncherConfirmStartVMPatch), nameof(EnableWithPrefix)));

            return res1;
        }

        public static bool EnableWithPrefix(Action? ____onConfirm)
        {
            ____onConfirm?.Invoke();
            return false;
        }
    }
}