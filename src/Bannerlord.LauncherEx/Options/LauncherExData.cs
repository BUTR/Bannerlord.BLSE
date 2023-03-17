namespace Bannerlord.LauncherEx.Options
{
    public sealed class LauncherExData
    {
        public bool AutomaticallyCheckForUpdates { get; set; }
        public bool FixCommonIssues { get; set; }
        public bool CompactModuleList { get; set; }
        public bool DisableBinaryCheck { get; set; }
        public bool HideRandomImage { get; set; }
        public bool BetaSorting { get; set; }
        public bool BigMode { get; set; }

        public LauncherExData() { }
        public LauncherExData(
            bool automaticallyCheckForUpdates,
            bool fixCommonIssues,
            bool compactModuleList,
            bool hideRandomImage,
            bool disableBinaryCheck,
            bool betaSorting,
            bool bigMode)
        {
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            FixCommonIssues = fixCommonIssues;
            CompactModuleList = compactModuleList;
            DisableBinaryCheck = disableBinaryCheck;
            HideRandomImage = hideRandomImage;
            BetaSorting = betaSorting;
            BigMode = bigMode;
        }
    }
}