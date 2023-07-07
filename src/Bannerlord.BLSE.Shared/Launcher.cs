using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.ExceptionInterceptor;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;
using System.Runtime.InteropServices;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared;

public static class Launcher
{
    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

    public static void Launch(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 6)
            PInvoke.SetProcessDPIAware();

        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        ModuleInitializer.Disable();

        _featureHarmony.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            prefix: SymbolExtensions2.GetMethodInfo(static (string[] x) => MainPrefix(ref x)));

        TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
    }

    private static void MainPrefix(ref string[] args)
    {
        ExceptionInterceptorFeature.Enable();
        ExceptionInterceptorFeature.EnableAutoGens();

        if (!args.Contains("no_watchdog"))
        {
            Array.Resize(ref args, args.Length + 1);
            args[args.Length - 1] = "no_watchdog";
        }

        SpecialKILoader.LoadSpecialKIfNeeded();
        ReShadeLoader.LoadReShadeIfNeeded();

        _featureHarmony.Unpatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            SymbolExtensions2.GetMethodInfo(static (string[] x) => MainPrefix(ref x)));
    }
}