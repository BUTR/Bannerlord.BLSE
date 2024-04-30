using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.ViewModels;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;
using Bannerlord.ModuleManager;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

using ApplicationVersion = TaleWorlds.Library.ApplicationVersion;

namespace Bannerlord.LauncherEx.Mixins;

// TODO:
internal enum ModuleType { Framework, Graphical, Standard, Patches }

internal sealed class LauncherModsVMMixin : ViewModelMixin<LauncherModsVMMixin, LauncherModsVM>/*, IHasOrderer*/
{
    private readonly BUTRLauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;
    private readonly MBBindingList<BUTRLauncherModuleVM> _modules = new();
    private readonly Dictionary<string, BUTRLauncherModuleVM> _modulesLookup = new();
    private IReadOnlyList<ModuleInfoExtendedWithMetadata> _allModuleInfos;

    [BUTRDataSourceProperty]
    public bool GlobalCheckboxState { get => _checkboxState; set => SetField(ref _checkboxState, value); }
    private bool _checkboxState;

    [BUTRDataSourceProperty]
    public bool IsDisabled2 { get => _isDisabled2; set => SetField(ref _isDisabled2, value); }
    private bool _isDisabled2;

    [BUTRDataSourceProperty]
    public MBBindingList<BUTRLauncherModuleVM> Modules2 { get; } = new();

    // Fast lookup for the ViewModels

    [BUTRDataSourceProperty]
    public bool IsForceSorted { get => _isForceSorted; set => SetField(ref _isForceSorted, value); }
    private bool _isForceSorted;

    [BUTRDataSourceProperty]
    public LauncherHintVM? ForceSortedHint { get => _forceSortedHint; set => SetField(ref _forceSortedHint, value); }
    private LauncherHintVM? _forceSortedHint;

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

    [BUTRDataSourceProperty]
    public string NameCategoryText2 => new BUTRTextObject("{=JtelOsIW}Name").ToString();
    [BUTRDataSourceProperty]
    public string VersionCategoryText2 => new BUTRTextObject("{=14WBFIS1}Version").ToString();

    [BUTRDataSourceProperty]
    public LauncherHintVM? GlobalCheckboxHint { get => _globalCheckboxHint; set => SetField(ref _globalCheckboxHint, value); }
    private LauncherHintVM? _globalCheckboxHint;

    [BUTRDataSourceProperty]
    public LauncherHintVM? RefreshHint { get => _refreshHint; set => SetField(ref _refreshHint, value); }
    private LauncherHintVM? _refreshHint;

    [BUTRDataSourceProperty]
    public LauncherHintVM? UpdateInfoHint { get => _updateInfoHint; set => SetField(ref _updateInfoHint, value); }
    private LauncherHintVM? _updateInfoHint;

    public LauncherModsVMMixin(LauncherModsVM launcherModsVM) : base(launcherModsVM)
    {
        GlobalCheckboxHint = new LauncherHintVM(new BUTRTextObject("{=q5quVWMI}Toggle All Modules").ToString());
        RefreshHint = new LauncherHintVM(new BUTRTextObject("{=H5nMY4WU}Refresh Modules").ToString());
        UpdateInfoHint = new LauncherHintVM(new BUTRTextObject("{=zXWdahH9}Get Update Recommendations{NL}Clicking on this button will send your module list to the BUTR server to get compatibility scores and recommended versions.{NL}They are based on the crash reports from ButterLib.{NL}{NL}(Requires Internet Connection)")
            .SetTextVariable("NL", Environment.NewLine).ToString());

        _launcherManagerHandler.RegisterModuleViewModelProvider(() => _modules, () => Modules2, SetViewModels);

        _launcherManagerHandler.RefreshModules();
        _allModuleInfos = _launcherManagerHandler.GetAllModules();
        foreach (var moduleInfoExtended in _launcherManagerHandler.ExtendedModuleInfoCache.Values.OfType<ModuleInfoExtendedWithMetadata>())
        {
            var moduleVM = new BUTRLauncherModuleVM(moduleInfoExtended, ToggleModuleSelection, ValidateModule, GetPossibleProviders);
            _modules.Add(moduleVM);
            _modulesLookup[moduleVM.ModuleInfoExtended.Id] = moduleVM;
        }
    }

    public ModuleInfoExtended? GetModuleById(string id) => _launcherManagerHandler.ExtendedModuleInfoCache.TryGetValue(id, out var mie) ? mie : null;
    public ModuleInfoExtended? GetModuleByName(string name) => _launcherManagerHandler.ExtendedModuleInfoCache.Values.FirstOrDefault(x => x.Name == name);

    public void Initialize()
    {
        Modules2.Clear();

        var loadOrder = _launcherManagerHandler.LoadLoadOrder().ToDictionary(x => x.Key, x => x.Value.IsSelected);

        /*
        if (!LoadOrderChecker.IsLoadOrderCorrect())
        {

        }
        */

        if (_launcherManagerHandler.TryOrderByLoadOrder(loadOrder.Keys, x => loadOrder.TryGetValue(x, out var isSelected) && isSelected, out var issues, out var orderedModules))
        {
            SetViewModels(orderedModules);
            IsForceSorted = false;
        }
        else
        {
            IsForceSorted = true;
            ForceSortedHint = new LauncherHintVM(new BUTRTextObject("{=pZVVdI5d}The Load Order was re-sorted with the default algorithm!{NL}Reasons:{NL}{REASONS}").SetTextVariable("REASONS", string.Join("\n", issues)).ToString());

            // Beta sorting algorithm will fail currently in some cases, use the TW fallback
            _launcherManagerHandler.TryOrderByLoadOrderTW(Enumerable.Empty<string>(), x => loadOrder.TryGetValue(x, out var isSelected) && isSelected, out _, out orderedModules, true);
            SetViewModels(orderedModules); // Set the ViewModels regarding the result

            //TryOrderByLoadOrder(Enumerable.Empty<string>(), x => loadOrder.TryGetValue(x, out var isSelected) && isSelected);
        }
    }

    private void SetViewModels(IEnumerable<IModuleViewModel> orderedModuleViewModels)
    {
        Modules2.Clear();
        foreach (var viewModel in orderedModuleViewModels.OfType<BUTRLauncherModuleVM>())
            Modules2.Add(viewModel);

        _launcherManagerHandler.RefreshModules();
        _allModuleInfos = _launcherManagerHandler.GetAllModules();

        // Validate all VM's after they were selected and ordered
        foreach (var modules in Modules2)
        {
            modules.Validate();
            modules.Refresh();
        }

        _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
    }

    private IEnumerable<string> ValidateModule(BUTRLauncherModuleVM moduleVM) => SortHelper.ValidateModule(Modules2, _modulesLookup, moduleVM);
    private void ToggleModuleSelection(BUTRLauncherModuleVM moduleVM)
    {
        SortHelper.ToggleModuleSelection(Modules2, _modulesLookup, moduleVM);
        _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
    }
    private ICollection<ModuleProviderType> GetPossibleProviders(ModuleInfoExtendedWithMetadata moduleInfo) => _allModuleInfos
        .Where(x => x.Id == moduleInfo.Id && x.ModuleProviderType != moduleInfo.ModuleProviderType)
        .Select(x => x.ModuleProviderType)
        .ToList();

    private void ChangeModulePosition(BUTRLauncherModuleVM targetModuleVM, int insertIndex, Action<IReadOnlyCollection<string>>? onIssues = null)
    {
        if (SortHelper.ChangeModulePosition(Modules2, _modulesLookup, targetModuleVM, insertIndex, onIssues))
        {
            _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
        }
    }

    private void SearchTextChanged()
    {
        var searchText = SearchText;
        if (string.IsNullOrEmpty(searchText))
        {
            foreach (var moduleVM in Modules2)
            {
                moduleVM.IsVisible = true;
            }
            return;
        }

        foreach (var moduleVM in Modules2)
        {
            moduleVM.IsVisible = moduleVM.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }

    [BUTRDataSourceMethod]
    public void ExecuteRefresh()
    {
        static IEnumerable<ModuleInfoExtended> Sort(IEnumerable<ModuleInfoExtended> source)
        {
            var orderedModules = source
                .OrderByDescending(x => x.IsOfficial)
                .ThenBy(x => x.Id, new AlphanumComparatorFast())
                .ToArray();

            return ModuleSorter.TopologySort(orderedModules, module => ModuleUtilities.GetDependencies(orderedModules, module));
        }

        var sorted = Sort(Modules2.Select(x => x.ModuleInfoExtended)).Select((x, i) => new { Item = x.Id, Index = i }).ToDictionary(x => x.Item, x => x.Index);
        Modules2.Sort(new ByIndexComparer<BUTRLauncherModuleVM>(x => sorted.TryGetValue(x.ModuleInfoExtended.Id, out var idx) ? idx : -1));
        //var sorted = Sort(Modules2.Select(x => x.ModuleInfoExtended)).Select(x => x.Id).ToList();
        //SortBy(sorted);

        _launcherManagerHandler.RefreshModules();
        _allModuleInfos = _launcherManagerHandler.GetAllModules();
        foreach (var moduleVM in Modules2)
            moduleVM.Refresh();

        _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
    }

    [BUTRDataSourceMethod]
    public void OnDrop(BUTRLauncherModuleVM targetModuleVM, int insertIndex, string type)
    {
        if (type == "Module")
        {
            ChangeModulePosition(targetModuleVM, insertIndex, issues =>
            {
                HintManager.ShowHint(new BUTRTextObject("{=sP1a61KE}Failed to place the module to the desired position! Placing to the nearest available!{NL}Reason:{NL}{REASONS}")
                    .SetTextVariable("REASONS", string.Join("\n", issues)).ToString());
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(5000);
                    HintManager.HideHint();
                }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
            });
        }
    }

    [BUTRDataSourceMethod]
    public void ExecuteUpdateCheck()
    {
        try
        {
            var uploadUrlAttr = typeof(LauncherModsVMMixin).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "BUTRCompatibilityScoreUrl");
            if (uploadUrlAttr is null)
                return;

            var gameVersion = ApplicationVersionHelper.GameVersion() ?? ApplicationVersion.Empty;
            var selectedModules = Modules2.Select(x => new
            {
                ModuleId = x.ModuleInfoExtended.Id,
                ModuleVersion = x.ModuleInfoExtended.Version.ToString()
            }).ToArray();
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                GameVersion = $"{ApplicationVersion.GetPrefix(gameVersion.ApplicationVersionType)}{gameVersion.Major}.{gameVersion.Minor}.{gameVersion.Revision}",
                Modules = selectedModules
            }));

            var responseDefinition = new { Modules = new[] { new { ModuleId = "", Compatibility = 0d, RecommendedCompatibility = (double?) null, RecommendedModuleVersion = (string?) null } } };

            var httpWebRequest = WebRequest.CreateHttp(uploadUrlAttr.Value);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.UserAgent = $"BLSE LauncherEx v{typeof(LauncherModsVMMixin).Assembly.GetName().Version}";
            httpWebRequest.Headers.Add("Tenant", "1");

            using var writeStream = httpWebRequest.GetRequestStream();
            writeStream.Write(data, 0, data.Length);

            using var response = httpWebRequest.GetResponse();
            using var stream = response.GetResponseStream();
            using var responseReader = new StreamReader(stream ?? Stream.Null);
            var json = responseReader.ReadLine() ?? string.Empty;
            var result = JsonConvert.DeserializeAnonymousType(json, responseDefinition);
            if (result is null) return;

            foreach (var moduleVM in Modules2)
                moduleVM.RemoveUpdateInfo();

            foreach (var module in result.Modules)
            {
                if (Modules2.FirstOrDefault(x => x.ModuleInfoExtended.Id == module.ModuleId) is not { } moduleVM) continue;
                moduleVM.SetUpdateInfo(module.Compatibility, module.RecommendedCompatibility, module.RecommendedModuleVersion);
            }
        }
        catch (Exception) { /* ignore */ }
    }

    [BUTRDataSourceMethod]
    public void ExecuteGlobalCheckbox()
    {
        GlobalCheckboxState = !GlobalCheckboxState;

        foreach (var moduleVM in Modules2)
        {
            if (GlobalCheckboxState)
            {
                if (moduleVM.IsValid && !moduleVM.IsSelected)
                    ToggleModuleSelection(moduleVM);
            }
            else
            {
                if (!moduleVM.ModuleInfoExtended.IsNative() && moduleVM.IsSelected)
                    ToggleModuleSelection(moduleVM);
            }
        }
    }
}