using Bannerlord.BLSE.Utils;
using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.AssemblyResolver.Patches
{
    internal static class AssemblyLoaderPatch
    {
        public static bool Enable(Harmony harmony)
        {
            return harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "OnAssemblyResolve"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(OnAssemblyResolvePrefix)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool OnAssemblyResolvePrefix(ref Assembly? __result, ResolveEventArgs args)
        {
            try
            {
                //var configName = Common.ConfigName;
                // We have Gaming.Desktop.x64_Shipping_Client for MSStore/Xbox, but in reality, it doesn't work.
                var configName = "Win64_Shipping_Client";
                
                var isInGame = GameUtils.GetModulesNames() is not null;

                var name = args.Name.Contains(',') ? $"{args.Name.Split(',')[0]}.dll" : args.Name;

                var assemblies = (isInGame ? ModuleInfoHelper.GetLoadedModules() : ModuleInfoHelper.GetModules()).Select(x =>
                {
                    var directory = Path.Combine(x.Path, "bin", configName);
                    return Directory.Exists(directory) ? Directory.GetFiles(directory, "*.dll") : Array.Empty<string>();
                }).ToArray();

                var assembly = assemblies
                    .SelectMany(x => x)
                    .FirstOrDefault(x => x.Contains(name));

                if (assembly is not null)
                {
                    __result = Assembly.LoadFrom(assembly);
                    return false;
                }
            }
            catch (Exception) { }

            return true;
        }
    }
}