using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Reflection;

namespace Bannerlord.BLSE.Features.ExceptionInterceptor
{
    // TaleWorlds.DotNet.Managed:ApplicationTick                              -> Replicated
    // TaleWorlds.Engine.ScriptComponentBehaviour:OnTick                      -> Called by TaleWorlds.Engine.ManagedScriptHolder:TickComponents
    // TaleWorlds.MountAndBlade.Module:OnApplicationTick                      -> Replicated
    // TaleWorlds.MountAndBlade.View.Missions.MissionView:OnMissionScreenTick -> Called by TaleWorlds.MountAndBlade.View.Screen.MissionScreen:OnFrameTick
    // TaleWorlds.ScreenSystem.ScreenManager:Tick                             -> Replicated
    // TaleWorlds.MountAndBlade.Mission:Tick                                  -> Replicated
    // TaleWorlds.MountAndBlade.MissionBehaviour:OnMissionTick                -> Called by TaleWorlds.MountAndBlade.Mission:Tick
    // TaleWorlds.MountAndBlade.MBSubModuleBase:OnSubModuleLoad               -> Replicated
    internal static class FinalizerSimple
    {
        private static readonly MethodInfo? ModuleInitializeMethod = AccessTools2.Method("TaleWorlds.MountAndBlade.Module:Initialize");

        private static readonly MethodInfo? ManagedApplicationTickMethod = AccessTools2.Method("TaleWorlds.DotNet.Managed:ApplicationTick");

        private static readonly MethodInfo? ScreenManagerPreTickMethod = AccessTools2.Method("TaleWorlds.Engine.EngineScreenManager:PreTick");
        private static readonly MethodInfo? ScreenManagerTickMethod = AccessTools2.Method("TaleWorlds.Engine.EngineScreenManager:Tick");
        private static readonly MethodInfo? ScreenManagerLateTickMethod = AccessTools2.Method("TaleWorlds.Engine.EngineScreenManager:LateTick");

        private static readonly MethodInfo? ManagedScriptHolderTickComponentsMethod = AccessTools2.Method("TaleWorlds.Engine.ManagedScriptHolder:TickComponents");

        public static void Enable(Harmony harmony, MethodInfo finalizerMethod)
        {
            harmony.Patch(ModuleInitializeMethod, finalizer: new HarmonyMethod(finalizerMethod));

            harmony.Patch(ManagedApplicationTickMethod, finalizer: new HarmonyMethod(finalizerMethod));

            harmony.Patch(ScreenManagerPreTickMethod, finalizer: new HarmonyMethod(finalizerMethod));
            harmony.Patch(ScreenManagerTickMethod, finalizer: new HarmonyMethod(finalizerMethod));
            harmony.Patch(ScreenManagerLateTickMethod, finalizer: new HarmonyMethod(finalizerMethod));

            harmony.Patch(ManagedScriptHolderTickComponentsMethod, finalizer: new HarmonyMethod(finalizerMethod));
        }
        }
}