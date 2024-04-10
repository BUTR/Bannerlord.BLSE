using Bannerlord.LauncherEx.Helpers;

using System.Xml;

namespace Bannerlord.LauncherEx.PrefabExtensions;

/// <summary>
/// Adds Options button on very top
/// </summary>
internal sealed class UILauncherPrefabExtension3 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/ListPanel/Children/ButtonWidget[1]";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension3()
    {
        XmlDocument.LoadXml(@"
<ButtonWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""110"" ButtonType=""Radio"" IsSelected=""@IsOptions"" UpdateChildrenStates=""true"">
  <Children>
    <TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.GameTypeButton.SingleplayerText"" Text=""@OptionsText"" />
  </Children>
</ButtonWidget>
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}

/// <summary>
/// Replaces original Divider with a left one
/// </summary>
internal sealed class UILauncherPrefabExtension4 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

    public override string Attribute => "MarginLeft";
    public override string Value => "125";
}

/// <summary>
/// Appends a second Divider to the right
/// </summary>
internal sealed class UILauncherPrefabExtension5 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "/Prefab/Window/LauncherDragWindowAreaWidget/Children/Widget/Children/Widget/Children/Widget[2]/Children/Widget[2]/Children/Widget[1]/Children/Widget[1]";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension5()
    {
        XmlDocument.LoadXml(@"
<Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""2"" SuggestedHeight=""30"" HorizontalAlignment=""Center"" MarginRight=""125"" Sprite=""top_header_divider"" />
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}

/// <summary>
/// Adds Launcher Tab on lower tab screen
/// </summary>
internal sealed class UILauncherPrefabExtension6 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::TabToggleWidget[2]";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension6()
    {
        XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" IsSelected=""@IsOptions"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""OptionsLauncherPage"" UpdateChildrenStates=""true"" IsVisible=""@IsOptions"">
  <Children>
    <TextWidget Id=""SubMenuText"" Text=""@LauncherText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
  </Children>
</TabToggleWidget>
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}
internal sealed class UILauncherPrefabExtension20 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::TabToggleWidget[3]";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension20()
    {
        XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""OptionsGamePage"" UpdateChildrenStates=""true"" IsVisible=""@IsOptions"">
  <Children>
    <TextWidget Id=""SubMenuText"" Text=""@GameText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
  </Children>
</TabToggleWidget>
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}
internal sealed class UILauncherPrefabExtension21 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::TabToggleWidget[4]";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension21()
    {
        XmlDocument.LoadXml(@"
<TabToggleWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""100"" TabControlWidget=""..\..\..\..\ContentPanel"" TabName=""OptionsEnginePage"" UpdateChildrenStates=""true"" IsVisible=""@IsOptions"">
  <Children>
    <TextWidget Id=""SubMenuText"" Text=""@EngineText"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""Launcher.SubMenuButton.SingleplayerText"" />
  </Children>
</TabToggleWidget>
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}

/// <summary>
/// Adds the Options Tab View
/// </summary>
internal sealed class UILauncherPrefabExtension7 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::Launcher.Mods";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension7()
    {
        XmlDocument.LoadXml(@"
<Launcher.Options Id=""OptionsLauncherPage"" DataSource=""{OptionsLauncherData}"" IsDisabled=""@IsDisabled"" />
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}
internal sealed class UILauncherPrefabExtension22 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::Launcher.Options[@Id='OptionsLauncherPage']";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension22()
    {
        XmlDocument.LoadXml(@"
<Launcher.Options Id=""OptionsGamePage"" DataSource=""{OptionsGameData}"" IsDisabled=""@IsDisabled"" />
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}
internal sealed class UILauncherPrefabExtension23 : PrefabExtensionInsertAsSiblingPatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::Launcher.Options[@Id='OptionsGamePage']";

    public override InsertType Type => InsertType.Append;
    private XmlDocument XmlDocument { get; } = new();

    public UILauncherPrefabExtension23()
    {
        XmlDocument.LoadXml(@"
<Launcher.Options Id=""OptionsEnginePage"" DataSource=""{OptionsEngineData}"" IsDisabled=""@IsDisabled"" />
");
    }

    public override XmlDocument GetPrefabExtension() => XmlDocument;
}

/// <summary>
/// Changing to Option screen will change image
/// </summary>
internal sealed class UILauncherPrefabExtension10 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "UILauncher";
    public static string XPath => "descendant::LauncherRandomImageWidget";

    public override string Attribute => "ChangeTrigger";
    public override string Value => "@RandomImageSwitch";
}