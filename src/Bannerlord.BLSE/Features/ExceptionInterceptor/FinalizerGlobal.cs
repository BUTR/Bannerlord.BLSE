using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BLSE.Features.ExceptionInterceptor;

internal static class FinalizerGlobal
{
    private static readonly HashSet<string> BlacklistedMethodStarts = new()
    {
        "GameNetwork_",
        "ManagedOptions_",
        "MBEditor_",
        "MBMultiplayerData_",
        "CrashInformationCollector_",
        "NativeParallelDriver_",
        "NativeObject_",
        "ManagedObject_",
        "DotNetObject_",
        "ManagedExtensions_",
    };
    private static readonly HashSet<string> BlacklistedMethods = new()
    {
        "Managed_SetStringArrayValueAtIndex",
        "Managed_SetCurrentStringReturnValueAsUnicode",
        "Managed_SetCurrentStringReturnValue",
        "Managed_PassCustomCallbackMethodPointers",
        "Managed_GetStringArrayValueAtIndex",
        "Managed_GetStringArrayLength",
    };


    public static void Enable(Harmony harmony, MethodInfo finalizerMethod)
    {
            var callbacksGeneratedTypes = AccessTools2.AllAssemblies().SelectMany(x => x.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated")));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods)
                .Where(x => !BlacklistedMethods.Contains(x.Name))
                .Where(x => !BlacklistedMethodStarts.Any(y => x.Name.StartsWith(y)));
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                harmony.Patch(method, finalizer: new HarmonyMethod(finalizerMethod));
        }

    public static void OnNewAssembly(Harmony harmony, MethodInfo finalizerMethod, Assembly assembly)
    {
            var callbacksGeneratedTypes = assembly.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated"));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods)
                .Where(x => !BlacklistedMethods.Contains(x.Name))
                .Where(x => !BlacklistedMethodStarts.Any(y => x.Name.StartsWith(y)));
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                harmony.Patch(method, finalizer: new HarmonyMethod(finalizerMethod));
        }
}