using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.ExceptionInterceptor;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.NoExceptions;
using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;
using System.Runtime.InteropServices;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared;

public static class LauncherEx
{
    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

    public static void Launch(string[] args)
    {
        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        Manager.Initialize();

        var settings = Manager.CurrentSettingsSnapshot();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 6 && settings is { EnableDPIScaling: true })
            PInvoke.SetProcessDPIAware();

        Manager.Enable();

        ModuleInitializer.Disable();

        _featureHarmony.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            prefix: SymbolExtensions2.GetMethodInfo(static () => MainPrefix()));

        if (args.Contains("/noexceptions"))
        {
            ProgramEx.Main(args);
        }
        else
        {
            TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
        }
    }

    private static void MainPrefix()
    {
        var disableCrashHandler = LauncherSettings.DisableCrashHandlerWhenDebuggerIsAttached && DebuggerUtils.IsDebuggerAttached();
        if (!disableCrashHandler)
        {
            ExceptionInterceptorFeature.Enable();
        }

        if (!LauncherSettings.DisableCatchAutoGenExceptions)
        {
            ExceptionInterceptorFeature.EnableAutoGens();
        }

        if (!LauncherSettings.UseVanillaCrashHandler)
        {
            // TODO: Use the CLI when available
#if !DEBUG
            // We do not copy System.Memory.dll and System.Runtime.CompilerServices.Unsafe.dll, which are required
            // They are included in Release with ILMerge
            WatchdogHandler.DisableTWWatchdog();
#endif
        }

        _featureHarmony.Unpatch(AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"), SymbolExtensions2.GetMethodInfo(static () => MainPrefix()));
    }
}