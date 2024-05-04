using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.ExceptionInterceptor;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
        
        GameOriginalEntrypointHandler.Initialize();

        _featureHarmony.TryPatch(
            SymbolExtensions2.GetMethodInfo((string[] args_) => TaleWorlds.Starter.Library.Program.Main(args_)),
            transpiler: SymbolExtensions2.GetMethodInfo(static () => MainTranspiler(null!)));

        GameLauncherEntrypointHandler.Entrypoint(args);
    }
    
    private static IEnumerable<CodeInstruction> MainTranspiler(IEnumerable<CodeInstruction> codeInstructions) => new []
    {
        new CodeInstruction(OpCodes.Ldarg_0),
        new CodeInstruction(OpCodes.Call, SymbolExtensions2.GetMethodInfo((string[] args) => Entrypoint(args))),
        new CodeInstruction(OpCodes.Ret),
    };

    private static int Entrypoint(string[] args)
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
        
        return GameEntrypointHandler.Entrypoint(args);
    }
}