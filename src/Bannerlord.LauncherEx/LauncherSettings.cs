using System.Diagnostics.CodeAnalysis;

namespace Bannerlord.LauncherEx;

[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class LauncherSettings
{
    public static bool AutomaticallyCheckForUpdates { get; set; } = false;
    public static bool FixCommonIssues { get; set; } = false;
    public static bool CompactModuleList { get; set; } = false;
    public static bool DisableBinaryCheck { get; set; } = false;
    public static bool HideRandomImage { get; set; } = false;
    public static bool BetaSorting { get; set; } = false;
    public static bool BigMode { get; set; } = true;
    public static bool EnableDPIScaling { get; set; } = true;
    public static bool DisableCrashHandlerWhenDebuggerIsAttached { get; set; } = false;
    public static bool DisableCatchAutoGenExceptions { get; set; } = true;
    public static bool UseVanillaCrashHandler { get; set; } = false;
}