using Bannerlord.LauncherManager.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Nito.AsyncEx;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Patches;

internal static class ProgramPatch
{
    public static bool Enable(Harmony harmony)
    {
        var res1 = harmony.TryPatch(
            AccessTools2.Method(typeof(Program), "AuxFinalize"),
            postfix: AccessTools2.DeclaredMethod(typeof(ProgramPatch), nameof(AuxFinalizePostfix)));
        if (!res1) return false;

        return true;
    }

    private static void AuxFinalizePostfix()
    {
        if (LauncherSettings.FixCommonIssues)
        {
            AsyncContext.Run(() => BUTRLauncherManagerHandler.Default.CheckForRootHarmonyAsync());
        }

        Manager.Disable();
    }

}