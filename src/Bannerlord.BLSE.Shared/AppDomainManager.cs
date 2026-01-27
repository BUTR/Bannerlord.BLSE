using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.Utils;

using HarmonyLib;

using System;
using System.Runtime.InteropServices;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared;

public static class AppDomainManager
{
    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");
  
    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version.Major >= 6)
            PInvoke.SetProcessDPIAware();
        
        LauncherExceptionHandler.Watch();

        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        ModuleInitializer.Disable();
    }
}