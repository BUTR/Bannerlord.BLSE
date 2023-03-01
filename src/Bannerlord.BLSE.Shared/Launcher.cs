using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;

using HarmonyLib;

using System;
using System.Runtime.InteropServices;

namespace Bannerlord.BLSE.Shared;

public static class Launcher
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

    public static void Launch(string[] args)
    {
        if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();

        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        ModuleInitializer.Disable();
        TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
    }
}