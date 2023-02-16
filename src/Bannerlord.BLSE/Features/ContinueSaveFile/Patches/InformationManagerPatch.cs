using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.ContinueSaveFile.Patches
{
    internal static class InformationManagerPatch
    {
        internal static bool SkipChange = false;

        public static bool Enable(Harmony harmony)
        {
            return harmony.TryPatch(
                original: AccessTools2.Method(typeof(InformationManager), "ShowInquiry"),
                prefix: AccessTools2.Method(typeof(InformationManagerPatch), nameof(Prefix)));
        }

        private static bool Prefix(InquiryData data)
        {
            if (SkipChange)
            {
                data.AffirmativeAction?.Invoke();
                return false;
            }
            return true;
        }
    }
}