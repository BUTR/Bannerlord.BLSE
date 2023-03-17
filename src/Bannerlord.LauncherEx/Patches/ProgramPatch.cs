using Bannerlord.LauncherManager.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Patches
{
    internal static class ProgramPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(Program), "AuxFinalize"),
                prefix: AccessTools2.DeclaredMethod(typeof(ProgramPatch), nameof(AuxFinalizePrefix)));
            if (!res1) return false;

            return true;
        }

        private static void AuxFinalizePrefix()
        {
            if (LauncherSettings.FixCommonIssues)
            {
                BUTRLauncherManagerHandler.Default.CheckForRootHarmony();
            }

            Manager.Disable();
        }

    }
}