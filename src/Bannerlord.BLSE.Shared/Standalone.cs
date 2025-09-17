using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.ExceptionInterceptor;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.SaveSystem;

using Windows.Win32;

using MessageBoxButtons = Bannerlord.BLSE.Shared.Utils.MessageBoxButtons;
using MessageBoxDefaultButton = Bannerlord.BLSE.Shared.Utils.MessageBoxDefaultButton;
using MessageBoxIcon = Bannerlord.BLSE.Shared.Utils.MessageBoxIcon;

namespace Bannerlord.BLSE.Shared;

public static class Standalone
{
    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

    private static string[] GetModules(MetaData metadata)
    {
        if (!metadata.TryGetValue("Modules", out var text))
        {
            return Array.Empty<string>();
        }
        return text.Split(';');
    }

    private static void TryLoadLoadOrderFromSaveFile(ref string[] args)
    {
        // If _MODULES_ arg is missing but a save file to load is specified, use the load order from the save file
        var hasModules = false;
        var saveFile = string.Empty;
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("_MODULES_"))
                hasModules = true;
            if (string.Equals(args[i], "/continuesave", StringComparison.OrdinalIgnoreCase) && args.Length > i + 1)
                saveFile = args[i + 1];
        }

        if (hasModules || string.IsNullOrEmpty(saveFile))
            return;

        // We need the initialization for Steam discovery to work
        LauncherPlatform.Initialize();
        Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
        try
        {
            if (MBSaveLoad.GetSaveFileWithName(saveFile) is not { IsCorrupted: false } saveGameFileInfo) return;

            var loadedModule = ModuleInfoHelper.GetModules();

            var moduleNames = GetModules(saveGameFileInfo.MetaData).ToArray();
            var existingModules = moduleNames.Select(x => loadedModule.FirstOrDefault(y => y.Name == x)).OfType<ModuleInfoExtended>().ToArray();
            var existingModulesByName = existingModules.ToDictionary(x => x.Name, x => x);

            var missingNames = moduleNames.Select(x => x).Except(existingModulesByName.Keys).ToArray();
            if (missingNames.Length == 0)
            {
                args = args.Concat(new[] { $"_MODULES_*{string.Join("*", existingModules.Select(x => x.Id))}*_MODULES_" }).ToArray();
                return;
            }

            var message = $"""
No modules were provided as an argument!
Tried to use the load order from the save file {saveFile}, but it was missing the following mods:
{string.Join(Environment.NewLine, missingNames)}
Press Yes to exit, press No to continue loading
""";
            switch (MessageBoxDialog.Show(message, "Error from BLSE!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
            {
                case MessageBoxResult.Yes:
                    Environment.Exit(1);
                    return;
            }
        }
        finally
        {
            LauncherPlatform.Destroy();
            Common.PlatformFileHelper = null;
        }
    }

    public static void Launch(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 6)
            PInvoke.SetProcessDPIAware();

        TryLoadLoadOrderFromSaveFile(ref args);

        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        ModuleInitializer.Disable();

        GameOriginalEntrypointHandler.Initialize();

        _featureHarmony.TryPatch(
            SymbolExtensions2.GetMethodInfo((string[] args_) => TaleWorlds.Starter.Library.Program.Main(args_)),
            transpiler: SymbolExtensions2.GetMethodInfo(static () => MainTranspiler(null!)));

        // Just to keep the original method call in the stacktrace
        TaleWorlds.Starter.Library.Program.Main(args);
    }

    private static IEnumerable<CodeInstruction> MainTranspiler(IEnumerable<CodeInstruction> codeInstructions) => new[]
    {
        new CodeInstruction(OpCodes.Ldarg_0),
        new CodeInstruction(OpCodes.Call, SymbolExtensions2.GetMethodInfo((string[] args) => Entrypoint(args))),
        new CodeInstruction(OpCodes.Ret),
    };

    private static int Entrypoint(string[] args)
    {
        var disableCrashHandler = !args.Contains("/enablecrashhandlerwhendebuggerisattached") && DebuggerUtils.IsDebuggerAttached();
        if (!disableCrashHandler)
            ExceptionInterceptorFeature.Enable();

        if (!args.Contains("/disableautogenexceptions"))
            ExceptionInterceptorFeature.EnableAutoGens();

        if (!args.Contains("/enablevanillacrashhandler") && !args.Contains("no_watchdog"))
        {
            Array.Resize(ref args, args.Length + 1);
            args[args.Length - 1] = "no_watchdog";
        }

        return GameEntrypointHandler.Entrypoint(args);
    }
}