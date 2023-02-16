using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;

namespace Bannerlord.BLSE.Utils
{
    internal static class GameUtils
    {
        private static readonly Lazy<Type?> EngineApplicationInterfaceType =
            new(() => AccessTools2.TypeByName("TaleWorlds.Engine.EngineApplicationInterface"));

        private static readonly Lazy<AccessTools.FieldRef<object>?> IUtilField =
            new(() => AccessTools2.StaticFieldRefAccess<object>(EngineApplicationInterfaceType.Value!, "IUtil"));

        private delegate string GetModulesCodeDelegate(object instance);

        public static string[]? GetModulesNames()
        {
            var iUtil = IUtilField.Value?.Invoke();
            var getModulesCode = AccessTools2.GetDelegate<GetModulesCodeDelegate>(iUtil, "GetModulesCode");
            return iUtil is not null && getModulesCode is not null ? getModulesCode(iUtil)?.Split('*') : null;
        }
    }
}