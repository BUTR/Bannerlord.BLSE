using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace Bannerlord.BLSE.Features.ContinueSaveFile.Patches
{
    internal static class SandBoxSubModulePatch
    {
        private delegate void TryLoadSaveDelegate(SaveGameFileInfo saveInfo, Action<LoadResult> onStartGame, Action? onCancel = null);

        public static Func<GameStartupInfo, string?>? GetSaveGameArg;

        private static Harmony? _harmony;

        public static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            return harmony.TryPatch(
                AccessTools2.DeclaredMethod("SandBox.SandBoxSubModule:OnInitialState"),
                prefix: AccessTools2.DeclaredMethod(typeof(SandBoxSubModulePatch), nameof(OnInitialStatePrefix)));
        }

        private static bool OnInitialStatePrefix(MBSubModuleBase __instance)
        {
            if (AccessTools2.GetDelegate<TryLoadSaveDelegate>("SandBox.SandBoxSaveHelper:TryLoadSave") is not { } tryLoadSave) return true;
            if (GetSaveGameArg?.Invoke(Module.CurrentModule.StartupInfo) is not { } saveFileName) return true;
            if (saveFileName.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)) saveFileName = saveFileName.Remove(saveFileName.Length - 4, 4);
            if (MBSaveLoad.GetSaveFileWithName(saveFileName) is not { } saveFile) return true;
            if (AccessTools2.TypeByName("SandBox.SandBoxSubModule") is not { } sandBoxSubModuleType) return true;
            if (AccessTools2.GetDelegate<Action<LoadResult>>(__instance, sandBoxSubModuleType, "StartGame") is not { } startGame) return true;

            using (var _ = new InformationManagerConfirmInquiryHandler())
                tryLoadSave(saveFile, startGame);

            _harmony?.Unpatch(
                AccessTools2.DeclaredMethod("SandBox.SandBoxSubModule:OnInitialState"),
                AccessTools2.DeclaredMethod(typeof(SandBoxSubModulePatch), nameof(OnInitialStatePrefix)));

            return false;
        }
    }
}