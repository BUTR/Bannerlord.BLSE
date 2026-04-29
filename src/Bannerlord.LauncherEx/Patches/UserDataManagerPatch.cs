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
        // Wrapped end-to-end: a Postfix that throws propagates into the original SaveUserData
        // caller, so a transient AV lock, OneDrive sync conflict, or read-only attribute on
        // LauncherData.xml would surface as the user's settings silently failing to persist.
        // The save itself is atomic: write to a sibling .tmp file then File.Replace into the
        // destination. If anything fails before the Replace, the on-disk file still holds the
        // content TaleWorlds wrote in its own SaveUserData implementation (which ran before this
        // postfix), so we never leave LauncherData.xml in a half-written state — the corruption
        // path that produced "load order resets every launch" reports.
        var tempPath = ____filePath + ".tmp";
        try
        {
            var xDoc = new XmlDocument();
            xDoc.Load(____filePath);
            var rootNode = xDoc.DocumentElement!;

            var xmlSerializer = new XmlSerializer(typeof(LauncherExData));
            using var xout = new StringWriter();
            using var writer = XmlWriter.Create(xout, new XmlWriterSettings { OmitXmlDeclaration = true });
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

            var xfrag = xDoc.CreateDocumentFragment();
            xfrag.InnerXml = xout.ToString();
            if (xfrag.FirstChild is null) return;
            foreach (var element in xfrag.FirstChild.ChildNodes.OfType<XmlElement>().ToList())
            {
                rootNode.AppendChild(element);
            }

            xDoc.Save(tempPath);

            // Replace is atomic on NTFS — either the destination has the new content or it has
            // the prior content; never partial. Move requires the destination to not exist, so
            // Replace is the right primitive here.
            File.Replace(tempPath, ____filePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Bannerlord.LauncherEx: failed to persist BLSE settings to '{____filePath}': {e}");
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { /* best effort cleanup */ }
        }
    }
}