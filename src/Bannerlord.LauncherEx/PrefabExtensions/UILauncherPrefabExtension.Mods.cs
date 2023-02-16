using Bannerlord.LauncherEx.Helpers;

using System.Collections.Generic;
using System.Xml;

namespace Bannerlord.LauncherEx.PrefabExtensions
{
    /// <summary>
    /// ModsPage - uses 'ShowMods' instead of 'IsMultiplayer'
    /// </summary>
    internal sealed class UILauncherPrefabExtension9 : PrefabExtensionSetAttributesPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabToggleWidget[2]";

        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsHidden", ""),
            new Attribute("IsVisible", "@ShowMods"),
            new Attribute("IsSelected", "@IsModsDataSelected"),
        };
    }

    /// <summary>
    /// Replaces Launcher.Mods with our own static implementation, since we add a lot of custom stuff anyway
    /// </summary>
    internal sealed class UILauncherPrefabExtension13 : PrefabExtensionReplacePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.Mods";

        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension13()
        {
            XmlDocument.LoadXml(@"
<Launcher.Mods2 Id=""ModsPage"" DataSource=""{ModsData}"" IsDisabled=""@IsDisabled2"" />
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
}