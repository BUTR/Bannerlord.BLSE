using Bannerlord.LauncherManager.Localization;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.IO;

using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace Bannerlord.LauncherEx.ResourceManagers
{
    internal static class FontFactoryManager
    {
        private static Harmony? _harmony;

        internal static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            harmony.Patch(
                AccessTools2.Method(typeof(FontFactory), "LoadAllFonts"),
                postfix: new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(FontFactoryManager), nameof(LoadAllFontsPostfix))));

            harmony.Patch(
                AccessTools2.Method(typeof(FontFactory), "GetMappedFontForLocalization"),
                prefix: new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(FontFactoryManager), nameof(GetMappedFontForLocalizationPrefix))));

            return true;
        }

        private delegate void SetSpriteNamesDelegate(SpriteData instance, Dictionary<string, Sprite> value);
        private static readonly SetSpriteNamesDelegate? SetSpriteNames =
            AccessTools2.GetPropertySetterDelegate<SetSpriteNamesDelegate>(typeof(SpriteData), "SpriteNames");

        private static SpriteData WithData(this SpriteData spriteData, string spriteName)
        {
            SetSpriteNames!(spriteData, new Dictionary<string, Sprite>
            {
                { spriteName, SpriteDataManager.CreateGeneric(spriteName)! }
            });
            return spriteData;
        }

        private static void LoadAllFontsPostfix(ref FontFactory __instance)
        {
            switch (BUTRLocalizationManager.ActiveLanguage)
            {
                case BUTRLocalizationManager.ChineseTraditional:
                case BUTRLocalizationManager.ChineseSimple:
                    __instance.AddFontDefinition(Path.Combine(BasePath.Name, "GUI", "GauntletUI", "Fonts", "simkai") + "/", "simkai", new SpriteData("simkai").WithData("simkai"));
                    break;
                case BUTRLocalizationManager.Japanese:
                    __instance.AddFontDefinition(Path.Combine(BasePath.Name, "GUI", "GauntletUI", "Fonts", "SourceHanSansJP") + "/", "SourceHanSansJP", new SpriteData("SourceHanSansJP").WithData("SourceHanSansJP"));
                    break;
                case BUTRLocalizationManager.Korean:
                    __instance.AddFontDefinition(Path.Combine(BasePath.Name, "GUI", "GauntletUI", "Fonts", "NanumGothicKR") + "/", "NanumGothicKR", new SpriteData("NanumGothicKR").WithData("NanumGothicKR"));
                    break;
            }

        }
        private static bool GetMappedFontForLocalizationPrefix(ref FontFactory __instance, ref Font __result)
        {
            switch (BUTRLocalizationManager.ActiveLanguage)
            {
                case BUTRLocalizationManager.ChineseTraditional:
                case BUTRLocalizationManager.ChineseSimple:
                    __result = __instance.GetFont("simkai");
                    return false;
                case BUTRLocalizationManager.Japanese:
                    __result = __instance.GetFont("SourceHanSansJP");
                    return false;
                case BUTRLocalizationManager.Korean:
                    __result = __instance.GetFont("NanumGothicKR");
                    return false;
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}