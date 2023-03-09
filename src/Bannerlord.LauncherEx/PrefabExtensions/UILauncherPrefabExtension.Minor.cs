using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.Mixins;

using System.Collections.Generic;
using System.Linq;
using System.Xml;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.PrefabExtensions
{
    /// <summary>
    /// BLSE text up the Version. Singleplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension1 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TextWidget[@Text='@VersionText']";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension1()
        {
            XmlDocument.LoadXml(@$"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""@BLSEVersionMarginBottom"" IsVisible=""@ShowBUTRLoaderVersionText"" Text=""@BLSEVersionText""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }
    /// <summary>
    /// BUTRLoader text up the Version. Singleplayer
    /// </summary>
    internal sealed class UILauncherPrefabExtension2 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TextWidget[@Text='@BLSEVersionText']";

        public override InsertType Type => InsertType.Append;
        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension2()
        {
            XmlDocument.LoadXml(@$"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" VerticalAlignment=""Bottom"" Brush=""Launcher.Version.Text"" MarginLeft=""7"" MarginBottom=""@BUTRLoaderVersionMarginBottom"" IsVisible=""@ShowBUTRLoaderVersionText"" Text=""@BUTRLoaderVersionText""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    /// <summary>
    /// Reset MarginRight of TopMenu widget
    /// </summary>
    internal sealed class UILauncherPrefabExtension15 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Widget[@Id='TopMenu']";

        public override string Attribute => "MarginRight";
        public override string Value => "10";
    }

    /// <summary>
    /// Move the random image to the left by 100 pixels
    /// </summary>
    internal sealed class UILauncherPrefabExtension16 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::LauncherRandomImageWidget";

        public override string Attribute => "MarginRight";
        public override string Value => "-100";
    }

    /// <summary>
    /// Make the random image hideable
    /// </summary>
    internal sealed class UILauncherPrefabExtension17 : PrefabExtensionSetAttributesPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::LauncherRandomImageWidget";

        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsHidden", ""),
            new Attribute("IsVisible", "@ShowRandomImage"),
        };
    }

    /// <summary>
    /// Set our dynamic margin for random image
    /// </summary>
    internal sealed class UILauncherPrefabExtension18 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabControl[@Id='ContentPanel']";

        public override string Attribute => "MarginRight";
        public override string Value => "@ContentTabControlMarginRight";
    }

    internal sealed class UILauncherPrefabExtension28 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::TabControl[@Id='ContentPanel']";

        public override string Attribute => "MarginBottom";
        public override string Value => "@ContentTabControlMarginBottom";
    }

    internal sealed class UILauncherPrefabExtension29 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[4]";

        public override string Attribute => "MarginBottom";
        public override string Value => "@DividerMarginBottom";
    }

    /// <summary>
    /// Replaces the standard values with our overrides
    /// </summary>
    internal sealed class UILauncherPrefabExtension19 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab";

        public override void Apply(XmlNode node)
        {
            foreach (var selectNode in node.OwnerDocument?.SelectNodes("//*")?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>())
            {
                foreach (var attribute in selectNode.Attributes?.OfType<XmlAttribute>() ?? Enumerable.Empty<XmlAttribute>())
                {
                    attribute.Value = attribute.Value switch
                    {
                        $"@{nameof(LauncherVM.IsSingleplayer)}" => $"@{nameof(LauncherVMMixin.IsSingleplayer2)}",
                        $"@{nameof(LauncherVM.IsMultiplayer)}" => $"@{nameof(LauncherVMMixin.IsMultiplayer2)}",
                        $"@{"IsDigitalCompanion"}" => $"@{nameof(LauncherVMMixin.IsDigitalCompanion2)}",
                        _ => attribute.Value
                    };
                }
            }
        }
    }

    internal sealed class UILauncherPrefabExtension30 : PrefabExtensionSetAttributePatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget";

        public override string Attribute => "SuggestedHeight";
        public override string Value => "@BackgroundHeight";
    }

    internal sealed class UILauncherPrefabExtension31 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab";

        public override void Apply(XmlNode node)
        {
            foreach (var selectNode in node.OwnerDocument?.SelectNodes("//*")?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>())
            {
                foreach (var attribute in selectNode.Attributes?.OfType<XmlAttribute>() ?? Enumerable.Empty<XmlAttribute>())
                {
                    if (attribute.Name != "Text") continue;
                    if (!attribute.Value.StartsWith("@")) continue;
                    if (!attribute.Value.EndsWith("Text")) continue;
                    if (attribute.Value == "@Text") continue;
                    if (attribute.Value == "@VersionText") continue;
                    attribute.Value = $"{attribute.Value}2";
                }
            }
        }
    }

    internal sealed class UILauncherPrefabExtension34 : PrefabExtensionCustomPatch<XmlNode>
    {
        public static string Movie => "UILauncher";
        public static string XPath => "/Prefab";

        public override void Apply(XmlNode node)
        {
            foreach (var selectNode in node.OwnerDocument?.SelectNodes("//*")?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>())
            {
                foreach (var attribute in selectNode.Attributes?.OfType<XmlAttribute>() ?? Enumerable.Empty<XmlAttribute>())
                {
                    if (attribute.Name != "Brush") continue;
                    if (attribute.Value != "Launcher.PlayButton.Text") continue;
                    if (attribute.Value == "@VersionText") continue;
                    attribute.Value = "Launcher.Button.Text";
                }
            }
        }
    }

    internal sealed class UILauncherPrefabExtension32 : PrefabExtensionInsertAsSiblingPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::Launcher.ConfirmStart";

        private XmlDocument XmlDocument { get; } = new();

        public UILauncherPrefabExtension32()
        {
            XmlDocument.LoadXml(@"
<Launcher.MessageBox DataSource=""{MessageBox}""/>
");
        }

        public override XmlDocument GetPrefabExtension() => XmlDocument;
    }

    // No more multiplayer for LauncherEx
    internal sealed class UILauncherPrefabExtension35 : PrefabExtensionSetAttributesPatch
    {
        public static string Movie => "UILauncher";
        public static string XPath => "descendant::ButtonWidget[@Id='PlayMultiplayerButton']";

        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsDisabled", "true"),
        };
    }
}