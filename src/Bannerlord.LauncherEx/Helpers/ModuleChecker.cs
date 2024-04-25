using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherManager.Models;
using Bannerlord.ModuleManager;

using Mono.Cecil;
using Mono.Cecil.Rocks;

using System;
using System.IO;
using System.Linq;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.Helpers;

// TODO: NativeAOT won't cover that?
internal static class ModuleChecker
{
    public static bool IsObfuscated(ModuleInfoExtendedWithMetadata moduleInfoExtended)
    {
        static bool CanBeLoaded(SubModuleInfoExtended x) =>
            ModuleInfoHelper.CheckIfSubModuleCanBeLoaded(x, ApplicationPlatform.CurrentPlatform, ApplicationPlatform.CurrentRuntimeLibrary, TaleWorlds.MountAndBlade.DedicatedServerType.None, false);

        foreach (var subModule in moduleInfoExtended.SubModules.Where(CanBeLoaded))
        {
            var asm = Path.GetFullPath(Path.Combine(moduleInfoExtended.Path, "bin", "Win64_Shipping_Client", subModule.DLLName));
            if (!File.Exists(asm)) continue;

            try
            {
                using var moduleDefinition = ModuleDefinition.ReadModule(asm);

                var hasObfuscationAttributeUsed = moduleDefinition.GetCustomAttributes().Any(x => x.Constructor.DeclaringType.Name switch
                {
                    "ConfusedByAttribute" => true,
                    _ => false
                });
                var hasObfuscationAttributeDeclared = moduleDefinition.Types.Any(x => x.Name switch
                {
                    "ConfusedByAttribute" => true,
                    _ => false
                });
                // Every module should have a module initializer. If it's missing, someone is hiding it
                var hasModuleInitializer = moduleDefinition.GetAllTypes().Any(x => x.Name == "<Module>");

                return hasObfuscationAttributeUsed || hasObfuscationAttributeDeclared || !hasModuleInitializer;
            }
            // Failing to read the metadata is a direct sign of metadata manipulation
            catch (Exception) { return true; }
        }

        return false;
    }
}