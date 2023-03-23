using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Reflection;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class GetEntryAssembly
{
    private static readonly Harmony _harmony = new("Bannerlord.BLSE.Shared.Patches.GetEntryAssembly");


    public static void Enable()
    {
        var result = _harmony.TryPatch(
            AccessTools2.Method(typeof(Assembly), nameof(Assembly.GetEntryAssembly)),
            prefix: AccessTools2.Method(typeof(Unblocker), nameof(GetEntryAssemblyPrefix)));
    }

    private static bool GetEntryAssemblyPrefix(ref Assembly __result)
    {
        __result = typeof(GetEntryAssembly).Assembly;
        return false;
    }
}