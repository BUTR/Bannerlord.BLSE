using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.Patches;
using Bannerlord.LauncherEx.ResourceManagers;
using Bannerlord.LauncherEx.TPac;
using Bannerlord.LauncherEx.Widgets;
using Bannerlord.LauncherManager.Localization;

using HarmonyLib;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

using TaleWorlds.Library;

[assembly: InternalsVisibleTo("Bannerlord.LauncherEx.Tests")]

// ReSharper disable once CheckNamespace
namespace Bannerlord.LauncherEx
{
    public static class Manager
    {
        //internal static readonly AssemblyCompatibilityChecker _compatibilityChecker = new();
        private static readonly Harmony _launcherHarmony = new("Bannerlord.LauncherEx");

        public static event Action? OnDisable;

        public static string GetActiveLanguage() => ConfigReader.GetGameOptions(path => File.Exists(path) ? File.ReadAllBytes(path) : null).TryGetValue("Language", out var lang) ? lang : "English";

        public static void Initialize()
        {
            //AssemblyLoaderPatch.Enable(_launcherHarmony);
        }

        public static void Enable()
        {
            ProgramPatch.Enable(_launcherHarmony);
            UserDataManagerPatch.Enable(_launcherHarmony);
            LauncherVMPatch.Enable(_launcherHarmony);
            LauncherModsVMPatch.Enable(_launcherHarmony);
            LauncherConfirmStartVMPatch.Enable(_launcherHarmony);
            LauncherUIPatch.Enable(_launcherHarmony);
            ViewModelPatch.Enable(_launcherHarmony);
            WidgetPrefabPatch.Enable(_launcherHarmony);

            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.LauncherEx.Resources.Localization.EN.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.LauncherEx.Resources.Localization.RU.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.LauncherEx.Resources.Localization.CNs.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.LauncherEx.Resources.Localization.TR.strings.xml"));
            BUTRLocalizationManager.LoadLanguage(Load("Bannerlord.LauncherEx.Resources.Localization.BR.strings.xml"));
            BUTRLocalizationManager.ActiveLanguage = GetActiveLanguage();

            GraphicsContextManager.Enable(_launcherHarmony);
            GraphicsContextManager.CreateAndRegister("launcher_arrow_down", LoadStream("Bannerlord.LauncherEx.Resources.Textures.arrow_down.png"));
            GraphicsContextManager.CreateAndRegister("launcher_arrow_left", LoadStream("Bannerlord.LauncherEx.Resources.Textures.arrow_left.png"));
            GraphicsContextManager.CreateAndRegister("launcher_export", LoadStream("Bannerlord.LauncherEx.Resources.Textures.export.png"));
            GraphicsContextManager.CreateAndRegister("launcher_import", LoadStream("Bannerlord.LauncherEx.Resources.Textures.import.png"));
            GraphicsContextManager.CreateAndRegister("launcher_refresh", LoadStream("Bannerlord.LauncherEx.Resources.Textures.refresh.png"));
            GraphicsContextManager.CreateAndRegister("launcher_folder", LoadStream("Bannerlord.LauncherEx.Resources.Textures.folder.png"));
            GraphicsContextManager.CreateAndRegister("launcher_search", LoadStream("Bannerlord.LauncherEx.Resources.Textures.search.png"));
            GraphicsContextManager.CreateAndRegister("warm_overlay", LoadStream("Bannerlord.LauncherEx.Resources.Textures.warm_overlay.png"));

            SpriteDataManager.Enable(_launcherHarmony);
            SpriteDataManager.CreateAndRegister("launcher_arrow_down");
            SpriteDataManager.CreateAndRegister("launcher_arrow_left");
            SpriteDataManager.CreateAndRegister("launcher_import");
            SpriteDataManager.CreateAndRegister("launcher_export");
            SpriteDataManager.CreateAndRegister("launcher_refresh");
            SpriteDataManager.CreateAndRegister("launcher_folder");
            SpriteDataManager.CreateAndRegister("launcher_search");
            SpriteDataManager.CreateAndRegister("warm_overlay");

            var asset = new AssetPackage(Path.Combine(BasePath.Name, ModuleInfoHelper.ModulesFolder, "Native/AssetPackages/gauntlet_ui.tpac"));
            switch (BUTRLocalizationManager.ActiveLanguage)
            {
                case BUTRLocalizationManager.ChineseTraditional or BUTRLocalizationManager.ChineseSimple when asset.GetTexture("ui_fonts_1") is { } chinese:
                    GraphicsContextManager.CreateAssetTextureAndRegister("simkai", chinese);
                    SpriteDataManager.CreateGenericAndRegister("simkai");
                    break;
                case BUTRLocalizationManager.Japanese when asset.GetTexture("ui_fonts_2") is { } japanese:
                    GraphicsContextManager.CreateAssetTextureAndRegister("SourceHanSansJP", japanese);
                    SpriteDataManager.CreateGenericAndRegister("SourceHanSansJP");
                    break;
                case BUTRLocalizationManager.Korean when asset.GetTexture("ui_fonts_4") is { } korean:
                    GraphicsContextManager.CreateAssetTextureAndRegister("NanumGothicKR", korean);
                    SpriteDataManager.CreateGenericAndRegister("NanumGothicKR");
                    break;
            }


            BrushFactoryManager.Enable(_launcherHarmony);
            BrushFactoryManager.CreateAndRegister(Load("Bannerlord.LauncherEx.Resources.Brushes.Launcher.xml"));

            WidgetFactoryManager.Enable(_launcherHarmony);
            WidgetFactoryManager.Register(typeof(LauncherToggleButtonWidget));
            WidgetFactoryManager.Register(typeof(LauncherSearchBoxWidget));

            WidgetFactoryManager.CreateAndRegister("Launcher.ToggleButton", Load("Bannerlord.LauncherEx.Resources.Prefabs.Widgets.Launcher.ToggleButton.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SearchBox", Load("Bannerlord.LauncherEx.Resources.Prefabs.Widgets.Launcher.SearchBox.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Scrollbar", Load("Bannerlord.LauncherEx.Resources.Prefabs.Widgets.Launcher.Scrollbar.xml"));

            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyBoolView", Load("Bannerlord.LauncherEx.Resources.Prefabs.Properties.Launcher.SettingsPropertyBoolView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyButtonView", Load("Bannerlord.LauncherEx.Resources.Prefabs.Properties.Launcher.SettingsPropertyButtonView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyFloatView", Load("Bannerlord.LauncherEx.Resources.Prefabs.Properties.Launcher.SettingsPropertyFloatView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyIntView", Load("Bannerlord.LauncherEx.Resources.Prefabs.Properties.Launcher.SettingsPropertyIntView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.SettingsPropertyStringView", Load("Bannerlord.LauncherEx.Resources.Prefabs.Properties.Launcher.SettingsPropertyStringView.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Options.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Options.OptionTuple", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Options.OptionTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods2", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Mods.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Mods.ModuleTuple2", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Mods.ModuleTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Saves.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.Saves.SaveTuple", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.Saves.SaveTuple.xml"));
            WidgetFactoryManager.CreateAndRegister("Launcher.MessageBox", Load("Bannerlord.LauncherEx.Resources.Prefabs.Launcher.MessageBox.xml"));

            FontFactoryManager.Enable(_launcherHarmony);
        }

        private static XmlDocument Load(string embedPath)
        {
            using var stream = typeof(Manager).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null) throw new Exception($"Could not find embed resource '{embedPath}'!");
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true });
            var doc = new XmlDocument();
            doc.Load(xmlReader);
            return doc;
        }
        private static Stream LoadStream(string embedPath)
        {
            return typeof(Manager).Assembly.GetManifestResourceStream(embedPath) ?? throw new Exception($"Could not find embed resource '{embedPath}'!");
        }

        public static void Disable()
        {
            OnDisable?.Invoke();
            //_compatibilityChecker.Dispose();
            GraphicsContextManager.Clear();
            SpriteDataManager.Clear();
            BrushFactoryManager.Clear();
            WidgetFactoryManager.Clear();
            BUTRLocalizationManager.Clear();
            _launcherHarmony.UnpatchAll(_launcherHarmony.Id);
        }
    }
}