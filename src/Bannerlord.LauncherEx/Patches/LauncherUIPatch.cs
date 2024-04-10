using Bannerlord.LauncherEx.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx.Patches;

internal static class LauncherUIPatch
{
    public static bool Enable(Harmony harmony)
    {
        var res1 = harmony.TryPatch(
            AccessTools2.DeclaredMethod(typeof(LauncherUI), "Initialize"),
            postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(InitializePostfix)));
        if (!res1) return false;

        var res2 = harmony.TryPatch(
            AccessTools2.DeclaredMethod(typeof(LauncherUI), "Update"),
            postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(UpdatePostfix)));
        if (!res2) return false;

        var res3 = harmony.TryPatch(
            AccessTools2.DeclaredPropertyGetter(typeof(LauncherUI), "AdditionalArgs"),
            postfix: AccessTools2.DeclaredMethod(typeof(LauncherUIPatch), nameof(AdditionalArgsPostfix)));
        if (!res3) return false;

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void InitializePostfix(GauntletMovie ____movie, LauncherVM ____viewModel, UserDataManager ____userDataManager)
    {
        BUTRLauncherManagerHandler.Initialize(____userDataManager);

        // Add to the existing VM our own properties
        MixinManager.AddMixins(____viewModel);
        ____movie.RefreshDataSource(____viewModel);
    }

    private static void UpdatePostfix(UIContext ____context)
    {
        if (Input.InputManager is BUTRInputManager butrInputManager)
        {
            if (____context.EventManager?.FocusedWidget is { } focusedWidget)
            {
                butrInputManager.Update();

                focusedWidget.HandleInput(butrInputManager.ReleasedChars);
            }
        }
        else
        {
            Input.Initialize(new BUTRInputManager(Input.InputManager), null);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AdditionalArgsPostfix()
    {
        if (Input.InputManager is BUTRInputManager butrInputManager)
        {
            Input.Initialize(butrInputManager.InputManager, null);
            butrInputManager.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
}