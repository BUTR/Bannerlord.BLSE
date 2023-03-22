using Bannerlord.BLSE.Features.Interceptor.Patches;
using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BLSE.Features.Interceptor
{
    public static class InterceptorFeature
    {
        public static string Id = FeatureIds.InterceptorId;

        private delegate void OnInitializeSubModulesPrefixDelegate();
        private delegate void OnLoadSubModulesPostfixDelegate();

        private static IEnumerable<Type> GetInterceptorTypes()
        {
            static bool CheckType(Type type) => type.GetCustomAttributes()
                .Any(att => string.Equals(att.GetType().FullName, typeof(BUTRLoaderInterceptorAttribute).FullName, StringComparison.Ordinal) ||
                            string.Equals(att.GetType().FullName, typeof(BLSEInterceptorAttribute).FullName, StringComparison.Ordinal));

            var dlls = new HashSet<string>(GetLoadedModulePaths());
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && dlls.Contains(x.Location)))
            {
                IEnumerable<Type> enumerable;
                try
                {
                    enumerable = assembly.GetTypes().Where(CheckType).ToArray(); // Force type resolution
                }
                catch (TypeLoadException)
                {
                    enumerable = Enumerable.Empty<Type>(); // ignore the incompatibility, not our problem
                }
                catch (ReflectionTypeLoadException)
                {
                    enumerable = Enumerable.Empty<Type>(); // ignore the incompatibility, not our problem
                }
                foreach (var type in enumerable)
                {
                    yield return type;
                }
            }
        }

        private static IEnumerable<string> GetLoadedModulePaths()
        {
            var configName = Common.ConfigName;

            foreach (var moduleInfo in ModuleInfoHelper.GetLoadedModules())
            {
                foreach (var subModule in moduleInfo.SubModules)
                {
                    if (ModuleInfoHelper.CheckIfSubModuleCanBeLoaded(subModule, ApplicationPlatform.CurrentPlatform, ApplicationPlatform.CurrentRuntimeLibrary, DedicatedServerType.None, false))
                    {
                        yield return System.IO.Path.GetFullPath(System.IO.Path.Combine(moduleInfo.Path, "bin", configName, subModule.DLLName));
                    }
                }
            }
        }

        public static void Enable(Harmony harmony)
        {
            ModulePatch.OnInitializeSubModulesPrefix += OnInitializeSubModulesPrefix;
            ModulePatch.OnLoadSubModulesPostfix += OnLoadSubModulesPostfix;
            ModulePatch.Enable(harmony);
        }

        private static void OnInitializeSubModulesPrefix()
        {
            foreach (var type in GetInterceptorTypes())
            {
                if (AccessTools2.GetDelegate<OnInitializeSubModulesPrefixDelegate>(type, "OnInitializeSubModulesPrefix") is { } method)
                {
                    method();
                }
            }
        }

        private static void OnLoadSubModulesPostfix()
        {
            foreach (var type in GetInterceptorTypes())
            {
                if (AccessTools2.GetDelegate<OnLoadSubModulesPostfixDelegate>(type, "OnLoadSubModulesPostfix") is { } method)
                {
                    method();
                }
            }
        }
    }
}