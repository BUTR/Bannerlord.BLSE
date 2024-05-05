using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class GameOriginalEntrypointHandler
{
    private static readonly Harmony _harmony = new("Bannerlord.BLSE.Shared.Utils.OriginalGameEntrypointHandler");

    [STAThread]
    public static int Entrypoint(string[] args) => throw new NotImplementedException("It's a stub");

    public static void Initialize()
    {
        _harmony.TryCreateReversePatcher(
            SymbolExtensions2.GetMethodInfo((string[] args_) => TaleWorlds.Starter.Library.Program.Main(args_)),
            SymbolExtensions2.GetMethodInfo((string[] x) => Entrypoint(x)))?.Patch();
    }
}