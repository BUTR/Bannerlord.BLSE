using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace Bannerlord.LauncherEx.Options;

public sealed class LauncherExData
{
    public static LauncherExData? FromUserDataXml(string path)
    {
            var xmlSerializer = new XmlSerializer(typeof(LauncherExData), new XmlRootAttribute("UserData"));
            try
            {
                using var xmlReader = XmlReader.Create(path);
                return (LauncherExData) xmlSerializer.Deserialize(xmlReader);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }

    public bool AutomaticallyCheckForUpdates { get; set; }
    public bool FixCommonIssues { get; set; }
    public bool CompactModuleList { get; set; }
    public bool DisableBinaryCheck { get; set; }
    public bool HideRandomImage { get; set; }
    public bool BetaSorting { get; set; }
    public bool BigMode { get; set; }
    public bool EnableDPIScaling { get; set; }
    public bool DisableCrashHandlerWhenDebuggerIsAttached { get; set; }
    public bool DisableCatchAutoGenExceptions { get; set; }
    public bool UseVanillaCrashHandler { get; set; }

    public LauncherExData() { }
    public LauncherExData(
        bool automaticallyCheckForUpdates,
        bool fixCommonIssues,
        bool compactModuleList,
        bool hideRandomImage,
        bool disableBinaryCheck,
        bool betaSorting,
        bool bigMode,
        bool enableDPIScaling,
        bool disableCrashHandlerWhenDebuggerIsAttached,
        bool disableCatchAutoGenExceptions,
        bool useVanillaCrashHandler)
    {
            AutomaticallyCheckForUpdates = automaticallyCheckForUpdates;
            FixCommonIssues = fixCommonIssues;
            CompactModuleList = compactModuleList;
            DisableBinaryCheck = disableBinaryCheck;
            HideRandomImage = hideRandomImage;
            BetaSorting = betaSorting;
            BigMode = bigMode;
            EnableDPIScaling = enableDPIScaling;
            DisableCrashHandlerWhenDebuggerIsAttached = disableCrashHandlerWhenDebuggerIsAttached;
            DisableCatchAutoGenExceptions = disableCatchAutoGenExceptions;
            UseVanillaCrashHandler = useVanillaCrashHandler;
        }
}