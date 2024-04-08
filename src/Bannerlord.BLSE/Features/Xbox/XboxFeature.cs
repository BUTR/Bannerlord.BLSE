using Bannerlord.BLSE.Features.Xbox.Patches;

using HarmonyLib;

using System.IO;

namespace Bannerlord.BLSE.Features.Xbox;

/// <summary>
/// We currently just bypass the sign in screen on launch. Don't think it's very safe, but it works for now.
/// </summary>
public static class XboxFeature
{
    public static string Id = FeatureIds.XboxId;

    public static void Enable(Harmony harmony)
    {
            if (Path.GetFileName(Directory.GetCurrentDirectory()) != "Gaming.Desktop.x64_Shipping_Client")
                return;

            ModulePatch.Enable(harmony);
        }
}