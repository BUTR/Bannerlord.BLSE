using Bannerlord.LauncherEx.Extensions;
using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.ViewModels;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx.Mixins;

internal sealed class LauncherVMMixin : ViewModelMixin<LauncherVMMixin, LauncherVM>
{
    private delegate void ExecuteConfirmUnverifiedDLLStartDelegate(LauncherVM instance);
    private static readonly ExecuteConfirmUnverifiedDLLStartDelegate? ExecuteConfirmUnverifiedDLLStartOriginal =
        AccessTools2.GetDelegate<ExecuteConfirmUnverifiedDLLStartDelegate>(typeof(LauncherVM), "ExecuteConfirmUnverifiedDLLStart");

    private delegate void ExecuteStartGameDelegate(LauncherVM instance, int mode);
    private static readonly ExecuteStartGameDelegate? ExecuteStartGame =
        AccessTools2.GetDelegate<ExecuteStartGameDelegate>(typeof(LauncherVM), "ExecuteStartGame");

    private static readonly AccessTools.FieldRef<LauncherVM, UserDataManager>? UserDataManagerFieldRef =
        AccessTools2.FieldRefAccess<LauncherVM, UserDataManager>("_userDataManager");

    private delegate void SetIsDigitalCompanionDelegate(LauncherVM instance, bool value);
    private static readonly SetIsDigitalCompanionDelegate? SetIsDigitalCompanion =
        AccessTools2.GetPropertySetterDelegate<SetIsDigitalCompanionDelegate>(typeof(LauncherVM), "IsDigitalCompanion");

    private delegate void UpdateAndSaveUserModsDataDelegate(LauncherVM instance, bool isMultiplayer);
    private static readonly UpdateAndSaveUserModsDataDelegate? UpdateAndSaveUserModsDataMethod =
        AccessTools2.GetDelegate<UpdateAndSaveUserModsDataDelegate>(typeof(LauncherVM), "UpdateAndSaveUserModsData");

    private delegate void RefreshDelegate(LauncherVM instance);
    private static readonly RefreshDelegate? Refresh =
        AccessTools2.GetDelegate<RefreshDelegate>(typeof(LauncherVM), "Refresh");


    private enum TopTabs { NONE, Singleplayer, Multiplayer, Options, DigitalCompanion }
    private TopTabs _state;

    [BUTRDataSourceProperty]
    public bool IsSingleplayer2
    {
        get => _state == TopTabs.Singleplayer;
        set
        {
                if (value && _state != TopTabs.Singleplayer && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Singleplayer;

                    SetState();
                }
            }
    }

    [BUTRDataSourceProperty]
    public bool IsMultiplayer2
    {
        get => _state == TopTabs.Multiplayer;
        set
        {
                if (value && _state != TopTabs.Multiplayer && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.Multiplayer;

                    SetState();
                }
            }
    }

    [BUTRDataSourceProperty]
    public bool IsOptions
    {
        get => _state == TopTabs.Options;
        set
        {
                if (value && _state != TopTabs.Options && ViewModel is not null)
                {
                    _state = TopTabs.Options;

                    SetState();
                }
            }
    }

    [BUTRDataSourceProperty]
    public bool IsDigitalCompanion2
    {
        get => _state == TopTabs.DigitalCompanion;
        set
        {
                if (value && _state != TopTabs.DigitalCompanion && ViewModel is not null)
                {
                    if (_state == TopTabs.Options)
                    {
                        SaveOptions();
                    }

                    _state = TopTabs.DigitalCompanion;

                    SetState();
                }
            }
    }

    [BUTRDataSourceProperty]
    public bool IsModsDataSelected
    {
        get => _isModsDataSelected;
        set
        {
                if (SetField(ref _isModsDataSelected, value))
                {
                    OnPropertyChanged(nameof(ShowImportExport));
                    OnPropertyChanged(nameof(ShowContinueSingleplayerButton));
                }
            }
    }
    private bool _isModsDataSelected;

    [BUTRDataSourceProperty]
    public bool IsSavesDataSelected
    {
        get => _isSavesDataSelected;
        set
        {
                if (SetField(ref _isSavesDataSelected, value))
                {
                    OnPropertyChanged(nameof(ShowPlaySingleplayerButton));
                    OnPropertyChanged(nameof(ShowContinueSingleplayerButton));
                    OnPropertyChanged(nameof(ShowImportExport));
                }
            }
    }
    private bool _isSavesDataSelected;

    [BUTRDataSourceProperty]
    public HorizontalAlignment PlayButtonAlignment => _state == TopTabs.Singleplayer ? HorizontalAlignment.Right : HorizontalAlignment.Center;

    [BUTRDataSourceProperty]
    public bool RandomImageSwitch { get => _randomImageSwitch; set => SetField(ref _randomImageSwitch, value); }
    private bool _randomImageSwitch;

    [BUTRDataSourceProperty]
    public string OptionsText { get => _optionsText; set => SetField(ref _optionsText, value); }
    private string _optionsText = new BUTRTextObject("{=yS5hbWCL}Options").ToString();

    [BUTRDataSourceProperty]
    public string LauncherText { get => _launcherText; set => SetField(ref _launcherText, value); }
    private string _launcherText = new BUTRTextObject("{=V66qoU6n}Launcher").ToString();

    [BUTRDataSourceProperty]
    public string GameText { get => _gameText; set => SetField(ref _gameText, value); }
    private string _gameText = new BUTRTextObject("{=ro4RMgyt}Game").ToString();

    [BUTRDataSourceProperty]
    public string EngineText { get => _engineText; set => SetField(ref _engineText, value); }
    private string _engineText = new BUTRTextObject("{=q4rQuTgG}Engine").ToString();

    [BUTRDataSourceProperty]
    public string SavesText { get => _savesText; set => SetField(ref _savesText, value); }
    private string _savesText = new BUTRTextObject("{=d5OjKcGE}Saves").ToString();

    [BUTRDataSourceProperty]
    public string BLSEVersionText { get => _blseVersionText; set => SetField(ref _blseVersionText, value); }
    private string _blseVersionText;

    [BUTRDataSourceProperty]
    public string BUTRLoaderVersionText { get => _butrLoaderVersionText; set => SetField(ref _butrLoaderVersionText, value); }
    private string _butrLoaderVersionText;

    [BUTRDataSourceProperty]
    public BUTRLauncherOptionsVM OptionsLauncherData { get => _optionsLauncherData; set => SetField(ref _optionsLauncherData, value); }
    private BUTRLauncherOptionsVM _optionsLauncherData;

    [BUTRDataSourceProperty]
    public BUTRLauncherOptionsVM OptionsGameData { get => _optionsGameData; set => SetField(ref _optionsGameData, value); }
    private BUTRLauncherOptionsVM _optionsGameData;

    [BUTRDataSourceProperty]
    public BUTRLauncherOptionsVM OptionsEngineData { get => _optionsEngineData; set => SetField(ref _optionsEngineData, value); }
    private BUTRLauncherOptionsVM _optionsEngineData;

    [BUTRDataSourceProperty]
    public BUTRLauncherSavesVM? SavesData { get => _savesData; set => SetField(ref _savesData, value); }
    private BUTRLauncherSavesVM? _savesData;

    [BUTRDataSourceProperty]
    public BUTRLauncherMessageBoxVM? MessageBox { get => _messageBox; set => SetField(ref _messageBox, value); }
    private BUTRLauncherMessageBoxVM? _messageBox = new();

    [BUTRDataSourceProperty]
    public bool ShowMods => IsSingleplayer2 || IsMultiplayer2;
    [BUTRDataSourceProperty]
    public bool ShowNews => IsSingleplayer2 || IsMultiplayer2 || IsDigitalCompanion2;

    [BUTRDataSourceProperty]
    public bool ShowRandomImage { get => _showRandomImage; set => SetField(ref _showRandomImage, value); }
    private bool _showRandomImage;

    [BUTRDataSourceProperty]
    public bool ShowImportExport => IsSingleplayer2 && (IsModsDataSelected || (IsSavesDataSelected && SavesData?.Selected is not null));

    [BUTRDataSourceProperty]
    public bool ShowBUTRLoaderVersionText => IsSingleplayer2 || IsOptions;

    [BUTRDataSourceProperty]
    public bool ShowPlaySingleplayerButton => IsSingleplayer2 && !IsSavesDataSelected;
    [BUTRDataSourceProperty]
    public bool ShowContinueSingleplayerButton => IsSingleplayer2 && (!IsSavesDataSelected || SavesData?.Selected is not null);

    [BUTRDataSourceProperty]
    public float ContentTabControlMarginRight { get => _contentTabControlMarginRight; set => SetField(ref _contentTabControlMarginRight, value); }
    private float _contentTabControlMarginRight = 0;

    [BUTRDataSourceProperty]
    public float ContentTabControlMarginBottom { get => _contentTabControlMarginBottom; set => SetField(ref _contentTabControlMarginBottom, value); }
    private float _contentTabControlMarginBottom = 114;

    [BUTRDataSourceProperty]
    public float BUTRLoaderVersionMarginBottom { get => _butrLoaderVersionMarginBottom; set => SetField(ref _butrLoaderVersionMarginBottom, value); }
    private float _butrLoaderVersionMarginBottom = 90;

    [BUTRDataSourceProperty]
    public float BLSEVersionMarginBottom { get => _blseLoaderVersionMarginBottom; set => SetField(ref _blseLoaderVersionMarginBottom, value); }
    private float _blseLoaderVersionMarginBottom = 70;

    [BUTRDataSourceProperty]
    public float DividerMarginBottom { get => _dividerMarginBottom; set => SetField(ref _dividerMarginBottom, value); }
    private float _dividerMarginBottom = 113;

    [BUTRDataSourceProperty]
    public float BackgroundHeight { get => _backgroundHeight; set => SetField(ref _backgroundHeight, value); }
    private float _backgroundHeight = 581; // 700

    [BUTRDataSourceProperty]
    public string SingleplayerText2 => new BUTRTextObject("{=Hk7FBBSa}Singleplayer").ToString();
    [BUTRDataSourceProperty]
    public string MultiplayerText2 => new BUTRTextObject("{=UOGhdUWE}Multiplayer").ToString();
    [BUTRDataSourceProperty]
    public string DigitalCompanionText2 => new BUTRTextObject("{=VDTcZpPr}Digital Companion").ToString();
    [BUTRDataSourceProperty]
    public string NewsText2 => new BUTRTextObject("{=Tg0If68v}News").ToString();
    [BUTRDataSourceProperty]
    public string ModsText2 => new BUTRTextObject("{=YGU9eXM0}Mods").ToString();
    [BUTRDataSourceProperty]
    public string PlayText2 => new BUTRTextObject("{=xYv4iv7C}PLAY").ToString();
    [BUTRDataSourceProperty]
    public string ContinueText2 => new BUTRTextObject("{=6B3iZLqR}CONTINUE").ToString();
    [BUTRDataSourceProperty]
    public string LaunchText2 => new BUTRTextObject("{=eUt6GKkQ}LAUNCH").ToString();

    private readonly UserDataManager? _userDataManager;
    private readonly LauncherModsVMMixin? _launcherModsVMMixin;
    private readonly BUTRLauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;

    private ModuleListHandler? _currentModuleListHandler;

    public LauncherVMMixin(LauncherVM launcherVM) : base(launcherVM)
    {
            _launcherManagerHandler.RegisterStateProvider(() => new LauncherState(isSingleplayer: IsSingleplayer2));

            _userDataManager = UserDataManagerFieldRef?.Invoke(launcherVM);

            var blseMetadata = AccessTools2.TypeByName("Bannerlord.BLSE.BLSEInterceptorAttribute")?.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            var launcherExMetadata = typeof(LauncherVMMixin).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            _blseVersionText = $"BLSE v{blseMetadata?.FirstOrDefault(x => x.Key == "BLSEVersion")?.Value ?? "0.0.0.0"}";
            _butrLoaderVersionText = $"LauncherEx v{launcherExMetadata.FirstOrDefault(x => x.Key == "LauncherExVersion")?.Value ?? "0.0.0.0"}";

            _optionsEngineData = new BUTRLauncherOptionsVM(OptionsType.Engine, SaveUserData, RefreshOptions);
            _optionsGameData = new BUTRLauncherOptionsVM(OptionsType.Game, SaveUserData, RefreshOptions);
            _optionsLauncherData = new BUTRLauncherOptionsVM(OptionsType.Launcher, SaveUserData, RefreshOptions);

            if (launcherVM.GetPropertyValue(nameof(LauncherVM.ModsData)) is LauncherModsVM lmvm && lmvm.GetMixin<LauncherModsVMMixin, LauncherModsVM>() is { } mixin)
            {
                _launcherModsVMMixin = mixin;

                _savesData = new BUTRLauncherSavesVM(mixin.GetModuleById, mixin.GetModuleByName);
                _savesData.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == "SaveSelected")
                    {
                        OnPropertyChanged(nameof(ShowImportExport));
                        OnPropertyChanged(nameof(ShowContinueSingleplayerButton));
                    }
                };
            }

            ShowRandomImage = !LauncherSettings.HideRandomImage;
            ContentTabControlMarginRight = LauncherSettings.HideRandomImage ? 5 : 114;
            BackgroundHeight = LauncherSettings.BigMode ? 700 : 581;

            IsDigitalCompanion2 = (bool?) launcherVM.GetPropertyValue("IsDigitalCompanion") ?? false;
            IsMultiplayer2 = launcherVM.IsMultiplayer;
            IsSingleplayer2 = launcherVM.IsSingleplayer;

            Refresh?.Invoke(launcherVM);
        }

    private void SetState()
    {
            if (ViewModel is null) return;

            OnPropertyChanged(nameof(IsSingleplayer2));
            OnPropertyChanged(nameof(IsMultiplayer2));
            OnPropertyChanged(nameof(IsOptions));
            OnPropertyChanged(nameof(IsDigitalCompanion2));
            OnPropertyChanged(nameof(ShowBUTRLoaderVersionText));
            OnPropertyChanged(nameof(PlayButtonAlignment));
            OnPropertyChanged(nameof(ShowNews));
            OnPropertyChanged(nameof(ShowMods));
            OnPropertyChanged(nameof(ShowPlaySingleplayerButton));
            OnPropertyChanged(nameof(ShowContinueSingleplayerButton));

            ViewModel.IsSingleplayer = IsSingleplayer2;
            ViewModel.IsMultiplayer = IsMultiplayer2;
            SetIsDigitalCompanion?.Invoke(ViewModel, IsDigitalCompanion2);

            RandomImageSwitch = !RandomImageSwitch;

            ViewModel.News.SetPropertyValue(nameof(LauncherNewsVMMixin.IsDisabled2), !ShowNews);
            ViewModel.ModsData.SetPropertyValue(nameof(LauncherModsVMMixin.IsDisabled2), !ShowMods);
            if (SavesData is not null)
                SavesData.IsDisabled = !IsSingleplayer2;
            OptionsLauncherData.IsDisabled = !IsOptions;
            OptionsGameData.IsDisabled = !IsOptions;
            OptionsEngineData.IsDisabled = !IsOptions;
            if (IsOptions)
                RefreshOptions();

            ContentTabControlMarginBottom = IsOptions ? 65 : 114;
            BUTRLoaderVersionMarginBottom = IsOptions ? 45 : 90;
            BLSEVersionMarginBottom = IsOptions ? 25 : 70;
            DividerMarginBottom = IsOptions ? 64 : 113;
        }

    public void RefreshOptions()
    {
            OptionsLauncherData.Refresh();
            OptionsGameData.Refresh();
            OptionsEngineData.Refresh();
        }

    public void SaveUserData()
    {
            if (ViewModel is null) return;

            ShowRandomImage = !LauncherSettings.HideRandomImage;
            ContentTabControlMarginRight = LauncherSettings.HideRandomImage ? 5 : 114;
            BackgroundHeight = LauncherSettings.BigMode ? 700 : 581;
            UpdateAndSaveUserModsDataMethod?.Invoke(ViewModel, IsMultiplayer2);
        }

    public void SaveOptions()
    {
            OptionsLauncherData.Save();
            OptionsGameData.Save();
            OptionsEngineData.Save();
        }

    // Ensure save is triggered when launching the game
    [BUTRDataSourceMethod]
    public void ExecuteConfirmUnverifiedDLLStart()
    {
            if (ViewModel is null) return;

            SaveUserData();
            ExecuteConfirmUnverifiedDLLStartOriginal?.Invoke(ViewModel);
        }

    [BUTRDataSourceMethod]
    public void ExecuteBeginHintImport()
    {
            if (IsSingleplayer2 && IsModsDataSelected)
            {
                HintManager.ShowHint(new BUTRTextObject("{=Aws9irMU}Import Load Order"));
            }
            if (IsSingleplayer2 && IsSavesDataSelected)
            {
                HintManager.ShowHint(new BUTRTextObject("{=4wKr76gx}Import Save's Load Order"));
            }
        }

    [BUTRDataSourceMethod]
    public void ExecuteBeginHintExport()
    {
            if (IsSingleplayer2 && IsModsDataSelected)
            {
                HintManager.ShowHint(new BUTRTextObject("{=XdZGqnFW}Export Current Load Order"));
            }
            if (IsSingleplayer2 && IsSavesDataSelected)
            {
                HintManager.ShowHint(new BUTRTextObject("{=G55IdM6M}Export Save's Load Order"));
            }
        }

    [BUTRDataSourceMethod]
    public void ExecuteEndHint()
    {
            HintManager.HideHint();
        }

    [BUTRDataSourceMethod]
    public void ExecuteImport()
    {
            if (ViewModel is null || _launcherModsVMMixin is null || UpdateAndSaveUserModsDataMethod is null) return;

            _currentModuleListHandler = new ModuleListHandler(_launcherManagerHandler);

            if (IsSingleplayer2 && IsModsDataSelected)
            {
                var thread = new Thread(() =>
                {
                    _currentModuleListHandler.Import(result =>
                    {
                        if (!result) return;
                        UpdateAndSaveUserModsDataMethod(ViewModel, false);
                        HintManager.ShowHint(new BUTRTextObject("{=eohqbvHU}Successfully imported list!"));
                    });
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            if (IsSingleplayer2 && IsSavesDataSelected && SavesData?.Selected?.Name is { } saveName)
            {
                var thread = new Thread(() =>
                {
                    _currentModuleListHandler.ImportSaveFile(saveName, result =>
                    {
                        if (!result) return;
                        UpdateAndSaveUserModsDataMethod(ViewModel, false);
                        HintManager.ShowHint(new BUTRTextObject("{=eohqbvHU}Successfully imported list!"));
                    });
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

    [BUTRDataSourceMethod]
    public void ExecuteExport()
    {
            if (ViewModel is null || _launcherModsVMMixin is null) return;

            _currentModuleListHandler = new ModuleListHandler(_launcherManagerHandler);

            if (IsSingleplayer2 && IsModsDataSelected)
            {
                var thread = new Thread(() =>
                {
                    _currentModuleListHandler.Export();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            if (IsSingleplayer2 && IsSavesDataSelected && SavesData?.Selected?.Name is { } saveName)
            {
                var thread = new Thread(() =>
                {
                    _currentModuleListHandler.ExportSaveFile(saveName);
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

    [BUTRDataSourceMethod(OverrideName = "ExecuteStartGame")]
    public void ExecuteStartGameOverride(int mode)
    {
            if (ViewModel is null || ExecuteStartGame is null) return;

            if (IsSavesDataSelected && SavesData?.Selected is { } saveVM)
            {
                _launcherManagerHandler.SetGameParameterSaveFile(saveVM.Name);
                if (saveVM.HasWarning || saveVM.HasError)
                {
                    var description = new StringBuilder();
                    if (saveVM.HasError)
                    {
                        description.Append(saveVM.ErrorHint?.Text ?? string.Empty);
                    }

                    if (saveVM is { HasError: true, HasWarning: true })
                    {
                        description.Append("\n");
                    }

                    if (saveVM.HasWarning)
                    {
                        description.Append(saveVM.WarningHint?.Text ?? string.Empty);
                    }

                    description.Append("\n\n");
                    description.Append(new BUTRTextObject("{=MlYQ0uX7}An unstable experience could occur."));
                    description.Append("\n");
                    description.Append(new BUTRTextObject("{=qvzptzrE}Do you wish to continue loading the save?"));

                    MessageBox?.Show(new BUTRTextObject("{=dDprK7Mz}WARNING").ToString(), description.ToString(), () => ExecuteStartGame(ViewModel, 0), null);
                    return;
                }

                ExecuteStartGame(ViewModel, 0);
                return;
            }

            if (mode == 1)
            {
                _launcherManagerHandler.SetGameParameterContinueLastSaveFile(true);

                ExecuteStartGame(ViewModel, 1);
                return;
            }

            ExecuteStartGame(ViewModel, mode);
        }
}