﻿using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using HarmonyLib;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.SaveSystem;

namespace Bannerlord.BLSE.Standalone
{
    public static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

        private static string[] GetModules(MetaData metadata)
        {
            if (!metadata.TryGetValue("Modules", out var text))
            {
                return Array.Empty<string>();
            }
            return text.Split(';');
        }

        private static void TryLoadLoadOrderFromSaveFile(ref string[] args)
        {
            // If _MODULES_ arg is missing but a save file to load is specified, use the load order from the save file
            var hasModules = false;
            var saveFile = string.Empty;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("_MODULES_"))
                    hasModules = true;
                if (string.Equals(args[i], "/continuesave", StringComparison.OrdinalIgnoreCase) && args.Length > i + 1)
                    saveFile = args[i + 1];
            }

            if (hasModules || string.IsNullOrEmpty(saveFile))
                return;

            LauncherPlatform.Initialize();
            Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
            try
            {
                if (MBSaveLoad.GetSaveFileWithName(saveFile) is not { IsCorrupted: false } saveGameFileInfo) return;

                var loadedModule = ModuleInfoHelper.GetModules();

                var moduleNames = GetModules(saveGameFileInfo.MetaData).ToArray();
                var existingModules = moduleNames.Select(x => loadedModule.FirstOrDefault(y => y.Name == x)).OfType<ModuleInfoExtended>().ToArray();
                var existingModulesByName = existingModules.ToDictionary(x => x.Name, x => x);

                var missingNames = moduleNames.Select(x => x).Except(existingModulesByName.Keys).ToArray();
                if (missingNames.Length == 0)
                {
                    args = args.Concat(new[] { $"_MODULES_*{string.Join("*", existingModules.Select(x => x.Id))}*_MODULES_" }).ToArray();
                    return;
                }

                var message = $@"No modules were provided as an argument!
Tried to use the load order from the save file {saveFile}, but it was missing the following mods:
{string.Join(Environment.NewLine, missingNames)}
Press Yes to exit, press No to continue loading";
                switch (MessageBox.Show(message, "Error from BLSE!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, (MessageBoxOptions) 0x40000))
                {
                    case DialogResult.Yes:
                        Environment.Exit(1);
                        return;
                }
            }
            finally
            {
                LauncherPlatform.Destroy();
                Common.PlatformFileHelper = null;
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            TryLoadLoadOrderFromSaveFile(ref args);

            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);
            ContinueSaveFileFeature.Enable(_featureHarmony);
            CommandsFeature.Enable(_featureHarmony);

            TaleWorlds.Starter.Library.Program.Main(args);
        }
    }
}