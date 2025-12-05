using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BLSE.Shared;

internal static class LauncherPlatformHelper
{
    private delegate void InitializeV1();
    private delegate void InitializeV2(List<string> args);
    
    private static readonly InitializeV1? _initializeV1;
    private static readonly InitializeV2? _initializeV2;

    static LauncherPlatformHelper()
    {
        _initializeV1 = AccessTools2.GetDeclaredDelegate<InitializeV1>(typeof(LauncherPlatform), "Initialize", logErrorInTrace: false);
        _initializeV2 = AccessTools2.GetDeclaredDelegate<InitializeV2>(typeof(LauncherPlatform), "Initialize", logErrorInTrace: false);
    }

    public static void Initialize(string[] args)
    {
        if (_initializeV1 is not null)
            _initializeV1();
        else if (_initializeV2 is not null)
            _initializeV2(args.ToList());
        else
            Trace.TraceError("Failed to find Initialize method for LauncherPlatformHelper");
    }
}