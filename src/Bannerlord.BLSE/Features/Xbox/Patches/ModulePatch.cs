using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

using Module = TaleWorlds.MountAndBlade.Module;

namespace Bannerlord.BLSE.Features.Xbox.Patches;

internal static class ModulePatch
{
    public static bool Enable(Harmony harmony)
    {
            var asm = Assembly.LoadFrom("TaleWorlds.MountAndBlade.Platform.GDK.dll");
            Trace.Assert(asm is not null);

            var res1 = harmony.TryPatch(
                AccessTools2.Constructor(typeof(Module)),
                prefix: AccessTools2.Method(typeof(ModulePatch), nameof(ShowedLoginScreenPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Platform.GDK.PlatformGDKSubModule:OnSubModuleLoad"),
                prefix: AccessTools2.Method(typeof(ModulePatch), nameof(OnSubModuleLoadPrefix)));
            if (!res2) return false;

            var res3 = harmony.TryPatch(
                AccessTools2.DeclaredMethod("TaleWorlds.MountAndBlade.Platform.GDK.PlatformGDKSubModule:OnApplicationTick"),
                prefix: AccessTools2.Method(typeof(ModulePatch), nameof(OnApplicationTickPrefix)));
            if (!res3) return false;

            return true;
        }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowedLoginScreenPrefix(ref bool ___ShowedLoginScreen)
    {
            ___ShowedLoginScreen = true;
        }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool OnSubModuleLoadPrefix()
    {
            Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
            return false;
        }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool OnApplicationTickPrefix()
    {
            return false;
        }
}