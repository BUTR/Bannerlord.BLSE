using Bannerlord.BUTR.Shared.Helpers;

using System.IO;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class PlatformHelper
{
    private static string ConfigName = Path.GetFileName(Directory.GetCurrentDirectory());
    private static string GameBasePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../"));

    public static bool IsSteam() => ConfigName == "Win64_Shipping_Client" && File.Exists(Path.Combine(GameBasePath, ModuleInfoHelper.ModulesFolder, "Native", "steam.target"));
    public static bool IsGog() => ConfigName == "Win64_Shipping_Client" && File.Exists(Path.Combine(GameBasePath, ModuleInfoHelper.ModulesFolder, "Native", "gog.target"));
    public static bool IsGdk() => ConfigName == "Gaming.Desktop.x64_Shipping_Client" && File.Exists(Path.Combine(GameBasePath, "appxmanifest.xml"));
    public static bool IsEpic() => ConfigName == "Win64_Shipping_Client" && File.Exists(Path.Combine(GameBasePath, ModuleInfoHelper.ModulesFolder, "Native", "epic.target"));
}