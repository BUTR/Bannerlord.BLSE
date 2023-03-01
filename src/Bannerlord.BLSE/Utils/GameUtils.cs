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
            // Don't use Trace, as this is used in critical code like AssemblyResolver.
            // The less we trigger custom code, the better
            // A custom Trace listener will break the resolver if it will trigger a recursive assembly resolution
            var getModulesCode = AccessTools2.GetDelegate<GetModulesCodeDelegate>(iUtil, "GetModulesCode", logErrorInTrace: false);
            return iUtil is not null && getModulesCode is not null ? getModulesCode(iUtil)?.Split('*') : null;
        }
    }
}