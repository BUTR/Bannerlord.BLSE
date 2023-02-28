using Bannerlord.LauncherEx.Extensions;
using Bannerlord.LauncherEx.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.Patches
{
    internal static class ViewModelPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredConstructor(typeof(ViewModel)),
                prefix: AccessTools2.DeclaredMethod(typeof(ViewModelPatch), nameof(ViewModelCtorPrefix)));
            if (!res1) return false;
            
            return true;
        }

        private static bool ViewModelCtorPrefix(ViewModel __instance, ref Type ____type, ref object ____propertiesAndMethods)
        {
            if (__instance is BUTRViewModel && ViewModelExtensions.DataSourceTypeBindingPropertiesCollectionCtor is { } ctor)
            {
                ____type = __instance.GetType();
                ____propertiesAndMethods = ctor(new Dictionary<string, PropertyInfo>(), new Dictionary<string, MethodInfo>());

                return false;
            }
            return true;
        }
    }
}