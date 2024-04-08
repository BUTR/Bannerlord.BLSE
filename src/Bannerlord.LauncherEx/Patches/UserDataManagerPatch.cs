using Bannerlord.LauncherEx.Options;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx.Patches;

internal static class UserDataManagerPatch
{
    public static bool Enable(Harmony harmony)
    {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(UserDataManager), "LoadUserData"),
                prefix: AccessTools2.DeclaredMethod(typeof(UserDataManagerPatch), nameof(LoadUserDataPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(UserDataManager), "SaveUserData"),
                postfix: AccessTools2.DeclaredMethod(typeof(UserDataManagerPatch), nameof(SaveUserDataPostfix)));
            if (!res2) return false;

            return true;
        }

    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool LoadUserDataPrefix(string ____filePath)
    {
            if (!File.Exists(____filePath))
            {
                return true;
            }

            var userDataOptions = LauncherExData.FromUserDataXml(____filePath) ?? new();
            LauncherSettings.AutomaticallyCheckForUpdates = userDataOptions.AutomaticallyCheckForUpdates;
            LauncherSettings.FixCommonIssues = userDataOptions.FixCommonIssues;
            LauncherSettings.CompactModuleList = userDataOptions.CompactModuleList;
            LauncherSettings.HideRandomImage = userDataOptions.HideRandomImage;
            LauncherSettings.DisableBinaryCheck = userDataOptions.DisableBinaryCheck;
            LauncherSettings.BetaSorting = userDataOptions.BetaSorting;
            LauncherSettings.BigMode = userDataOptions.BigMode;
            LauncherSettings.EnableDPIScaling = userDataOptions.EnableDPIScaling;
            LauncherSettings.DisableCrashHandlerWhenDebuggerIsAttached = userDataOptions.DisableCrashHandlerWhenDebuggerIsAttached;
            LauncherSettings.DisableCatchAutoGenExceptions = userDataOptions.DisableCatchAutoGenExceptions;
            LauncherSettings.UseVanillaCrashHandler = userDataOptions.UseVanillaCrashHandler;

            return true;
        }

    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For Resharper")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SaveUserDataPostfix(string ____filePath)
    {
            var xDoc = new XmlDocument();
            xDoc.Load(____filePath);
            var rootNode = xDoc.DocumentElement!;

            var xmlSerializer = new XmlSerializer(typeof(LauncherExData));
            using var xout = new StringWriter();
            using var writer = XmlWriter.Create(xout, new XmlWriterSettings { OmitXmlDeclaration = true });
            try
            {
                xmlSerializer.Serialize(writer, new LauncherExData(
                    LauncherSettings.AutomaticallyCheckForUpdates,
                    LauncherSettings.FixCommonIssues,
                    LauncherSettings.CompactModuleList,
                    LauncherSettings.HideRandomImage,
                    LauncherSettings.DisableBinaryCheck,
                    LauncherSettings.BetaSorting,
                    LauncherSettings.BigMode,
                    LauncherSettings.EnableDPIScaling,
                    LauncherSettings.DisableCrashHandlerWhenDebuggerIsAttached,
                    LauncherSettings.DisableCatchAutoGenExceptions,
                    LauncherSettings.UseVanillaCrashHandler));
            }
            catch (Exception value)
            {
                Trace.WriteLine(value);
            }

            var xfrag = xDoc.CreateDocumentFragment();
            xfrag.InnerXml = xout.ToString();
            foreach (var element in xfrag.FirstChild.ChildNodes.OfType<XmlElement>().ToList())
            {
                rootNode.AppendChild(element);
            }

            xDoc.Save(____filePath);
        }
}