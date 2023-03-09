using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.ViewModels;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Mixins
{
    internal enum ModuleType { Framework, Graphical, Standard, Patches }

    internal sealed class LauncherModsVMMixin : ViewModelMixin<LauncherModsVMMixin, LauncherModsVM>/*, IHasOrderer*/
    {
        private readonly BUTRLauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;
        private readonly MBBindingList<BUTRLauncherModuleVM> _modules = new();
        private readonly Dictionary<string, BUTRLauncherModuleVM> _modulesLookup = new();

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

        public LauncherModsVMMixin(LauncherModsVM launcherModsVM) : base(launcherModsVM)
        {
            _launcherManagerHandler.RegisterModuleViewModelProvider(() => _modules, () => Modules2, SetViewModels);

            _launcherManagerHandler.RefreshModules();
            foreach (var moduleInfoExtended in _launcherManagerHandler.ExtendedModuleInfoCache.Values.OfType<ModuleInfoExtendedWithPath>())
            {
                var moduleVM = new BUTRLauncherModuleVM(moduleInfoExtended, ToggleModuleSelection, ValidateModule);
                _modules.Add(moduleVM);
                _modulesLookup[moduleVM.ModuleInfoExtended.Id] = moduleVM;
            }
        }

        public ModuleInfoExtended? GetModuleById(string id) => _launcherManagerHandler.ExtendedModuleInfoCache.TryGetValue(id, out var mie) ? mie : null;
        public ModuleInfoExtended? GetModuleByName(string name) => _launcherManagerHandler.ExtendedModuleInfoCache.Values.FirstOrDefault(x => x.Name == name);

        public void Initialize()
        {
            Modules2.Clear();

            var loadOrder = _launcherManagerHandler.LoadTWLoadOrder().ToDictionary(x => x.Key, x => x.Value.IsSelected);
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
            _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
        }

        private void SetViewModels(IEnumerable<IModuleViewModel> orderedModuleViewModels)
        {
            Modules2.Clear();
            foreach (var viewModel in orderedModuleViewModels.OfType<BUTRLauncherModuleVM>())
                Modules2.Add(viewModel);

            // Validate all VM's after they were selected and ordered
            foreach (var modules in Modules2)
                modules.Validate();
        }

        private IEnumerable<string> ValidateModule(BUTRLauncherModuleVM moduleVM) => SortHelper.ValidateModule(Modules2, _modulesLookup, moduleVM);
        private void ToggleModuleSelection(BUTRLauncherModuleVM moduleVM)
        {
            SortHelper.ToggleModuleSelection(Modules2, _modulesLookup, moduleVM);
            _launcherManagerHandler.SetGameParametersLoadOrder(Modules2);
        }

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
}