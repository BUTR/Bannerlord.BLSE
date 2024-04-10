using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherManager;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.ModuleManager;

using System;
using System.Linq;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.ViewModels;

internal sealed class BUTRLauncherSavesVM : BUTRViewModel
{
    private readonly LauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;
    private readonly Func<string, ModuleInfoExtended?> _getModuleById;
    private readonly Func<string, ModuleInfoExtended?> _getModuleByName;

    [BUTRDataSourceProperty]
    public string NameCategoryText { get => _nameCategoryText; set => SetField(ref _nameCategoryText, value); }
    private string _nameCategoryText = new BUTRTextObject("{=JtelOsIW}Name").ToString();

    [BUTRDataSourceProperty]
    public string VersionCategoryText { get => _versionCategoryText; set => SetField(ref _versionCategoryText, value); }
    private string _versionCategoryText = new BUTRTextObject("{=14WBFIS1}Version").ToString();

    [BUTRDataSourceProperty]
    public string CharacterNameCategoryText { get => _characterNameCategoryText; set => SetField(ref _characterNameCategoryText, value); }
    private string _characterNameCategoryText = new BUTRTextObject("{=OJsGrGVi}Character").ToString();

    [BUTRDataSourceProperty]
    public string LevelCategoryText { get => _levelCategoryText; set => SetField(ref _levelCategoryText, value); }
    private string _levelCategoryText = new BUTRTextObject("{=JxpEEQdF}Level").ToString();

    [BUTRDataSourceProperty]
    public string DaysCategoryText { get => _daysCategoryText; set => SetField(ref _daysCategoryText, value); }
    private string _daysCategoryText = new BUTRTextObject("{=qkkTPycE}Days").ToString();

    [BUTRDataSourceProperty]
    public string CreatedAtCategoryText { get => _createdAtCategoryText; set => SetField(ref _createdAtCategoryText, value); }
    private string _createdAtCategoryText = new BUTRTextObject("{=aYWWDkKX}CreatedAt").ToString();

    [BUTRDataSourceProperty]
    public MBBindingList<BUTRLauncherSaveVM> Saves { get; } = new();

    [BUTRDataSourceProperty]
    public bool IsDisabled { get => _isDisabled; set => SetField(ref _isDisabled, value); }
    private bool _isDisabled;

    [BUTRDataSourceProperty]
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                SearchTextChanged();
            }
        }
    }
    private string _searchText = string.Empty;

    public BUTRLauncherSaveVM? Selected => Saves.FirstOrDefault(x => x.IsSelected);

    public BUTRLauncherSavesVM(Func<string, ModuleInfoExtended?> getModuleById, Func<string, ModuleInfoExtended?> getModuleByName)
    {
        _getModuleById = getModuleById;
        _getModuleByName = getModuleByName;

        ExecuteRefresh();
    }

    private void SelectSave(BUTRLauncherSaveVM saveVM)
    {
        var previousState = saveVM.IsSelected;
        foreach (var save in Saves)
        {
            save.IsSelected = false;
        }
        saveVM.IsSelected = !previousState;
        OnPropertyChanged("SaveSelected");
    }

    private void SearchTextChanged()
    {
        var searchText = SearchText;
        if (string.IsNullOrEmpty(searchText))
        {
            foreach (var saveVM in Saves)
            {
                saveVM.IsVisible = true;
            }
            return;
        }

        foreach (var saveVM in Saves)
        {
            saveVM.IsVisible = saveVM.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }

    [BUTRDataSourceMethod]
    public void ExecuteRefresh()
    {
        Saves.Clear();
        foreach (var saveFile in _launcherManagerHandler.GetSaveFiles())
        {
            Saves.Add(new BUTRLauncherSaveVM(saveFile, SelectSave, _getModuleById, _getModuleByName));
        }
    }
}