using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BLSE.Features.AssemblyResolver.Patches;

/// <summary>
/// Reports the arguments the game is started with. Every loader starts the game through
/// TaleWorlds.Starter.Library.Program.Main, the vanilla launcher included, so this sees the final module list.
/// </summary>
internal static class ProgramPatch
{
    public static event Action<string[]>? OnMain;

    public static bool Enable(Harmony harmony) => harmony.TryPatch(
        AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
        prefix: AccessTools2.DeclaredMethod(typeof(ProgramPatch), nameof(MainPrefix)));

    private static void MainPrefix(object[] __args)
    {
        if (__args.Length > 0 && __args[0] is string[] args)
            OnMain?.Invoke(args);
    }
}
