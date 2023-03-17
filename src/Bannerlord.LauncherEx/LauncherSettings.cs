using System.Diagnostics.CodeAnalysis;

namespace Bannerlord.LauncherEx
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class LauncherSettings
    {
        public static bool AutomaticallyCheckForUpdates { get; set; }
        public static bool FixCommonIssues { get; set; }
        public static bool CompactModuleList { get; set; }
        public static bool DisableBinaryCheck { get; set; }
        public static bool HideRandomImage { get; set; }
        public static bool BetaSorting { get; set; } = true;
        public static bool BigMode { get; set; } = true;
    }
}