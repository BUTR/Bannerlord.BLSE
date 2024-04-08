using Bannerlord.BLSE.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;

using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BLSE.Features.ContinueSaveFile.Patches;

internal static class ModulePatch
{
    public static event Action<GameStartupInfo, string>? OnSaveGameArgParsed;

    private static Harmony? _harmony;

    public static bool Enable(Harmony harmony)
    {
            _harmony = harmony;

            return harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(Module), "ProcessApplicationArguments"),
                postfix: AccessTools2.DeclaredMethod(typeof(ModulePatch), nameof(ProcessApplicationArgumentsPostfix)));
        }

    private static void ProcessApplicationArgumentsPostfix(Module __instance)
    {
            var cli = Utilities.GetFullCommandLineString();
            var array = CommandLineSplitter.SplitCommandLine(cli).ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                if (!string.Equals(array[i], "/continuesave", StringComparison.OrdinalIgnoreCase)) continue;
                if (array.Length <= i + 1) continue;
                var saveGame = array[i + 1];
                OnSaveGameArgParsed?.Invoke(__instance.StartupInfo, saveGame);
            }

            _harmony?.Unpatch(
                AccessTools2.DeclaredMethod(typeof(Module), "ProcessApplicationArguments"),
                AccessTools2.DeclaredMethod(typeof(ModulePatch), nameof(ProcessApplicationArgumentsPostfix)));
        }
}