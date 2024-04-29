using Bannerlord.LauncherEx.Helpers;

using System.Collections.Generic;

namespace Bannerlord.LauncherEx.PrefabExtensions;

internal sealed class ModuleTuplePrefabExtension4 : PrefabExtensionSetAttributesPatch
{
    public static string Movie => "Launcher.Mods.ModuleTuple2";
    public static string XPath => "descendant::ListPanel[@Id='EntryPanel']";

    public override List<Attribute> Attributes => new()
    {
        new Attribute("SuggestedHeight", LauncherSettings.CompactModuleList ? "24" : "26"),
        new Attribute("MarginBottom", LauncherSettings.CompactModuleList ? "2" : "10"),
    };
}
internal sealed class ModuleTuplePrefabExtension6 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Mods.ModuleTuple2";
    public static string XPath => "descendant::TextWidget[@Text='@Name']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension7 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Mods.ModuleTuple2";
    public static string XPath => "descendant::TextWidget[@Text='@VersionText']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}


internal sealed class ModuleTuplePrefabExtension14 : PrefabExtensionSetAttributesPatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::ListPanel[@Id='EntryPanel']";

    public override List<Attribute> Attributes => new()
    {
        new Attribute("SuggestedHeight", LauncherSettings.CompactModuleList ? "24" : "26"),
        new Attribute("MarginBottom", LauncherSettings.CompactModuleList ? "2" : "10"),
    };
}
internal sealed class ModuleTuplePrefabExtension8 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@Name']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension9 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@Version']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension10 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@CharacterName']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension11 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@Level']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension12 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@Days']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}
internal sealed class ModuleTuplePrefabExtension13 : PrefabExtensionSetAttributePatch
{
    public static string Movie => "Launcher.Saves.SaveTuple";
    public static string XPath => "descendant::TextWidget[@Text='@CreatedAt']";

    public override string Attribute => "Brush.FontSize";
    public override string Value => LauncherSettings.CompactModuleList ? "20" : "26";
}