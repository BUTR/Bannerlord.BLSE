using Bannerlord.BLSE.Utils;
using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.AssemblyResolver
{
    public static class AssemblyResolverFeature
    {
        private static ResolveEventHandler AssemblyLoaderOnAssemblyResolve =
            AccessTools2.GetDelegate<ResolveEventHandler>(typeof(AssemblyLoader), "OnAssemblyResolve")!;
        
        public static string Id = FeatureIds.AssemblyResolverId;

        public static void Enable(Harmony harmony)
        {
            AssemblyLoader.Initialize();
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyLoaderOnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }
        
        private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            if (args.Name is null) return null;

            var name = new AssemblyName(args.Name);
            if (name.Name is null) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == name.FullName)
                {
                    return assembly;
                }
            }

            try
            {
                var configName = Common.ConfigName;

                var isInGame = GameUtils.GetModulesNames() is not null;

                var assemblies = (isInGame ? ModuleInfoHelper.GetLoadedModules() : ModuleInfoHelper.GetModules()).Select(x =>
                {
                    var directory = Path.Combine(x.Path, "bin", configName);
                    return Directory.Exists(directory) ? Directory.GetFiles(directory, "*.dll") : Array.Empty<string>();
                }).ToArray();

                var assembly = assemblies
                    .SelectMany(x => x)
                    .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == name.Name);

                if (assembly is not null)
                {
                    return Assembly.LoadFrom(assembly);
                }
            }
            catch (Exception)
            {
                return null;
            }
 
            return null;
        }
    }
}