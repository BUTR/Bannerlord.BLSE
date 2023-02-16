using Bannerlord.LauncherEx.Helpers;

using System.Collections.Generic;

namespace Bannerlord.LauncherEx.PrefabExtensions
{
    /// <summary>
    /// ModsPage - uses 'ShowNews' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension8 : PrefabExtensionSetAttributesPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[1]";

        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsHidden", ""),
            new Attribute("IsVisible", "@ShowNews"),
        };
    }

    /// <summary>
    /// News tab can be disabled
    /// </summary>
    internal sealed class UILauncherPrefabExtension12 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.News";

        public override string Attribute => "IsDisabled";
        public override string Value => "@IsDisabled2";
    }
}