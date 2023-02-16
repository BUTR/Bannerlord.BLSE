using Bannerlord.LauncherManager.Localization;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Helpers
{
    internal static class HintManager
    {
        public static void ShowHint(BUTRTextObject message) => ShowHint(message.ToString());
        public static void ShowHint(string message) => LauncherUI.AddHintInformation(message);

        public static void HideHint() => LauncherUI.HideHintInformation();
    }
}