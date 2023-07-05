using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Linq;
using System.Reflection;

namespace Bannerlord.BLSE.Features.ExceptionInterceptor
{
    internal static class FinalizerGlobal
    {
        public static void Enable(Harmony harmony, MethodInfo finalizerMethod)
        {
            var callbacksGeneratedTypes = AccessTools2.AllAssemblies().SelectMany(x => x.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated")));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods);
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                harmony.Patch(method, finalizer: new HarmonyMethod(finalizerMethod));
        }

        public static void OnNewAssembly(Harmony harmony, MethodInfo finalizerMethod, Assembly assembly)
        {
            var callbacksGeneratedTypes = assembly.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated"));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods);
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                harmony.Patch(method, finalizer: new HarmonyMethod(finalizerMethod));
        }
    }
}