using Bannerlord.BUTR.Shared.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.BLSE.Utils;

internal static class TypeFinder
{
    public static IEnumerable<Type> GetInterceptorTypes(Type attributeType)
    {
        bool CheckType(Type type) => type.GetCustomAttributes()
            .Any(att => string.Equals(att.GetType().FullName, attributeType.FullName, StringComparison.Ordinal));

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
}