using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Runtime.CompilerServices;

using TaleWorlds.MountAndBlade;

namespace Bannerlord.BLSE.Features.Xbox.Patches
{
    internal static class ModulePatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Constructor(typeof(Module)),
                prefix: AccessTools2.Method(typeof(ModulePatch), nameof(ShowedLoginScreenPrefix)));
            if (!res1) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ShowedLoginScreenPrefix(ref bool ___ShowedLoginScreen)
        {
            ___ShowedLoginScreen = true;
        }
    }
}