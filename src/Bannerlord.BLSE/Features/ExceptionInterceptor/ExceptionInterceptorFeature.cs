using Bannerlord.BLSE.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;

namespace Bannerlord.BLSE.Features.ExceptionInterceptor
{
    public static class ExceptionInterceptorFeature
    {
        public static string Id = FeatureIds.ExceptionInterceptorId;

        private delegate void OnExceptionDelegate(Exception exception);

        private static readonly Harmony ExceptionHandler = new("bannerlord.blse.exceptionhandler");
        private static readonly MethodInfo? FinalizerMethod = AccessTools2.Method(typeof(ExceptionInterceptorFeature), nameof(Finalizer));

        public static event Action<Exception>? OnException;

        public static void Enable()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            OnException += HandleException;
        }

        public static void EnableAutoGens()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;
            var callbacksGeneratedTypes = AccessTools2.AllAssemblies().SelectMany(x => x.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated")));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods);
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                ExceptionHandler.Patch(method, finalizer: new HarmonyMethod(FinalizerMethod));
        }

        public static void Disable()
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            OnException -= HandleException;
            AppDomain.CurrentDomain.AssemblyLoad -= CurrentDomainOnAssemblyLoad;
            ExceptionHandler.UnpatchAll(ExceptionHandler.Id);
        }

        private static void CurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var assembly = args.LoadedAssembly;
            var callbacksGeneratedTypes = assembly.GetTypes().Where(y => y.Name.EndsWith("CallbacksGenerated"));
            var callbackGeneratedMethods = callbacksGeneratedTypes.SelectMany(AccessTools.GetDeclaredMethods);
            foreach (var method in callbackGeneratedMethods.Where(x => x.GetCustomAttributesData().Any(y => y.AttributeType.Name == "MonoPInvokeCallbackAttribute")))
                ExceptionHandler.Patch(method, finalizer: new HarmonyMethod(FinalizerMethod));
        }

        private static void Finalizer(Exception? __exception)
        {
            if (__exception is not null)
                HandleException(__exception);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static void CurrentDomainOnUnhandledException(object? _, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                OnException?.Invoke(exception);
        }

        private static void HandleException(Exception exception)
        {
            try
            {
                foreach (var type in TypeFinder.GetInterceptorTypes(typeof(BLSEExceptionHandlerAttribute)))
                {
                    if (AccessTools2.GetDelegate<OnExceptionDelegate>(type, "OnException") is { } method)
                    {
                        method(exception);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}