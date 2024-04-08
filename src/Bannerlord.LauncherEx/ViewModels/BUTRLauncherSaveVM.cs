using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherManager;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;
using Bannerlord.ModuleManager;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

using ApplicationVersion = Bannerlord.ModuleManager.ApplicationVersion;

namespace Bannerlord.LauncherEx.ViewModels;

internal sealed class BUTRLauncherSaveVM : BUTRViewModel
{
    private record ModuleListEntry(string Name, ApplicationVersion Version);

    [BUTRDataSourceProperty]
    public string Name { get => _name; set => SetField(ref _name, value); }
    private string _name;

    [BUTRDataSourceProperty]
    public string Version { get => _version; set => SetField(ref _version, value); }
    private string _version;

    [BUTRDataSourceProperty]
    public string CharacterName { get => _characterName; set => SetField(ref _characterName, value); }
    private string _characterName;

    [BUTRDataSourceProperty]
    public string Level { get => _level; set => SetField(ref _level, value); }
    private string _level;

    [BUTRDataSourceProperty]
    public string Days { get => _days; set => SetField(ref _days, value); }
    private string _days;

    [BUTRDataSourceProperty]
    public string CreatedAt { get => _createdAt; set => SetField(ref _createdAt, value); }
    private string _createdAt;

    [BUTRDataSourceProperty]
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
    private bool _isSelected;

    [BUTRDataSourceProperty]
    public LauncherHintVM? LoadOrderHint { get => _loadOrderHint; set => SetField(ref _loadOrderHint, value); }
    private LauncherHintVM? _loadOrderHint;

    [BUTRDataSourceProperty]
    public bool HasWarning { get => _hasWarning; set => SetField(ref _hasWarning, value); }
    private bool _hasWarning;

    [BUTRDataSourceProperty]
    public LauncherHintVM? WarningHint { get => _warningHint; set => SetField(ref _warningHint, value); }
    private LauncherHintVM? _warningHint;

    [BUTRDataSourceProperty]
    public bool HasError { get => _hasError; set => SetField(ref _hasError, value); }
    private bool _hasError;

    [BUTRDataSourceProperty]
    public LauncherHintVM? ErrorHint { get => _errorHint; set => SetField(ref _errorHint, value); }
    private LauncherHintVM? _errorHint;

    [BUTRDataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value); }
    private bool _isVisible = true;

    public string? ModuleListCode { get; private set; }

    private readonly LauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;
    private readonly SaveMetadata _saveMetadata;
    private readonly Action<BUTRLauncherSaveVM> _select;
    private readonly Func<string, ModuleInfoExtended?> _getModuleById;
    private readonly Func<string, ModuleInfoExtended?> _getModuleByName;

    public BUTRLauncherSaveVM(SaveMetadata saveMetadata, Action<BUTRLauncherSaveVM> select, Func<string, ModuleInfoExtended?> getModuleById, Func<string, ModuleInfoExtended?> getModuleByName)
    {
            _saveMetadata = saveMetadata;
            _select = select;
            _getModuleById = getModuleById;
            _getModuleByName = getModuleByName;

            _name = _saveMetadata.Name;
            _version = _saveMetadata.TryGetValue("ApplicationVersion", out var appVersion) && !string.IsNullOrEmpty(appVersion) ? string.Join(".", appVersion.Split('.').Take(3)) : "Save Old";
            _characterName = _saveMetadata.TryGetValue("CharacterName", out var characterName) && !string.IsNullOrEmpty(characterName) ? characterName : "Save Old";
            _level = _saveMetadata.TryGetValue("MainHeroLevel", out var level) && !string.IsNullOrEmpty(level) ? level : "Save Old";
            _days = _saveMetadata.TryGetValue("DayLong", out var daysr) && !string.IsNullOrEmpty(daysr) && float.TryParse(daysr, out var days) ? days.ToString("0") : "Save Old";
            _createdAt = _saveMetadata.TryGetValue("CreationTime", out var ctr) && !string.IsNullOrEmpty(ctr) && long.TryParse(ctr, out var ticks) ? new DateTime(ticks).ToString("d") : "Save Old";

            ValidateSave();
        }

    private void ValidateSave()
    {
            var changeset = _saveMetadata.GetChangeSet();
            var modules = _saveMetadata.GetModules().Select(x =>
            {
                var version = _saveMetadata.GetModuleVersion(x);
                if (version.ChangeSet == changeset)
                    version = new ApplicationVersion(version.ApplicationVersionType, version.Major, version.Minor, version.Revision, 0);
                return new ModuleListEntry(x, version);
            }).ToArray();

            var existingModules = modules.Select(x => _getModuleByName(x.Name)).OfType<ModuleInfoExtended>().ToArray();
            var nameDuplicates = existingModules.Select(x => x.Name).GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (nameDuplicates.Count > 0)
            {
                HasError = true;
                ErrorHint = new LauncherHintVM(new BUTRTextObject("{=vCwH9226}Duplicate Module Names:{NL}{MODULENAMES}")
                    .SetTextVariable("MODULENAMES", string.Join("\n", nameDuplicates)).ToString());
                return;
            }
            var existingModulesByName = existingModules.ToDictionary(x => x.Name, x => x);

            ModuleListCode = $"_MODULES_*{string.Join("*", existingModules.Select(x => x.Id))}*_MODULES_";

            var missingNames = modules.Select(x => x.Name).Except(existingModulesByName.Keys).ToArray();
            var loadOrderIssues = LoadOrderChecker.IsLoadOrderCorrect(existingModules).ToList();

            //LoadOrderHint = new LauncherHintVM($"Load Order:\n{string.Join("\n", existingModules.Select(x => x.Id))}\n\nUnknown Mod Names:{string.Join("\n", missingNames)}");
            LoadOrderHint = new LauncherHintVM(new BUTRTextObject("{=sd6M4KRd}Load Order:{NL}{LOADORDER}")
                .SetTextVariable("LOADORDER", string.Join("\n", modules.Select(x => _getModuleByName(x.Name)?.Id ?? $"{x.Name} {new BUTRTextObject("{=kxqLbSqe}(Unknown ID)")}"))).ToString());

            if (missingNames.Length > 0 || loadOrderIssues.Count > 0)
            {
                var text = string.Empty;
                if (loadOrderIssues.Count > 0)
                {
                    text += new BUTRTextObject("{=HvvA78sZ}Load Order Issues:{NL}{LOADORDERISSUES}")
                        .SetTextVariable("LOADORDERISSUES", string.Join("\n\n", loadOrderIssues));
                    text += missingNames.Length > 0 ? "\n\n\n" : string.Empty;
                }
                if (missingNames.Length > 0)
                {
                    text += new BUTRTextObject("{=GtDRbC3m}Missing Modules:{NL}{MODULES}")
                        .SetTextVariable("MODULES", string.Join("\n", missingNames)).ToString();
                }

                HasError = true;
                ErrorHint = new LauncherHintVM(text);
                return;
            }

            var issues = new List<string>();
            foreach (var module in modules)
            {
                var existingModule = existingModulesByName[module.Name];
                if (module.Version != existingModule.Version)
                {
                    issues.Add(new BUTRTextObject("{=nYVWoomO}{MODULEID}. Required {REQUIREDVERSION}. Actual {ACTUALVERSION}")
                        .SetTextVariable("MODULEID", existingModule.Id)
                        .SetTextVariable("REQUIREDVERSION", module.Version.ToString())
                        .SetTextVariable("ACTUALVERSION", existingModule.Version.ToString()).ToString());
                }
            }
            if (issues.Count > 0)
            {
                HasWarning = true;
                WarningHint = new LauncherHintVM(new BUTRTextObject("{=BuMom4Jt}Mismatched Module Versions:{NL}{MODULEVERSIONS}")
                    .SetTextVariable("MODULEVERSIONS", string.Join("\n\n", issues)).ToString());
            }
        }

    [BUTRDataSourceMethod]
    public void ExecuteSelect()
    {
            _select(this);
        }

    [BUTRDataSourceMethod]
    public void ExecuteOpen()
    {
            var saveFilePath = _launcherManagerHandler.GetSaveFilePath(_saveMetadata.Name);
            if (string.IsNullOrEmpty(saveFilePath) || !File.Exists(saveFilePath)) return;

            Process.Start("explorer.exe", $"/select,\"{saveFilePath}\"");
        }
}