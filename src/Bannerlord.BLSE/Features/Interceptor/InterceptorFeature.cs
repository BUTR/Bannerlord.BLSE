using Bannerlord.BLSE.Features.Interceptor.Patches;
using Bannerlord.BLSE.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

namespace Bannerlord.BLSE.Features.Interceptor
{
    public static class InterceptorFeature
    {
        public static string Id = FeatureIds.InterceptorId;

        private delegate void OnInitializeSubModulesPrefixDelegate();
        private delegate void OnLoadSubModulesPostfixDelegate();

        public static void Enable(Harmony harmony)
        {
            ModulePatch.OnInitializeSubModulesPrefix += OnInitializeSubModulesPrefix;
            ModulePatch.OnLoadSubModulesPostfix += OnLoadSubModulesPostfix;
            ModulePatch.Enable(harmony);
        }

        private static void OnInitializeSubModulesPrefix()
        {
            foreach (var type in TypeFinder.GetInterceptorTypes(typeof(BLSEInterceptorAttribute)))
            {
                if (AccessTools2.GetDelegate<OnInitializeSubModulesPrefixDelegate>(type, "OnInitializeSubModulesPrefix", logErrorInTrace: false) is { } method)
                {
                    method();
                }
            }
        }

        private static void OnLoadSubModulesPostfix()
        {
            foreach (var type in TypeFinder.GetInterceptorTypes(typeof(BLSEInterceptorAttribute)))
            {
                if (AccessTools2.GetDelegate<OnLoadSubModulesPostfixDelegate>(type, "OnLoadSubModulesPostfix", logErrorInTrace: false) is { } method)
                {
                    method();
                }
            }
        }
    }
}