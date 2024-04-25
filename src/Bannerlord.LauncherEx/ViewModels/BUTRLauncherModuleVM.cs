using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.ViewModels;

internal sealed class BUTRLauncherModuleVM : BUTRViewModel, IModuleViewModel
{
    private readonly Action<BUTRLauncherModuleVM> _select;
    private readonly Func<BUTRLauncherModuleVM, IEnumerable<string>> _validate;
    private readonly Func<ModuleInfoExtendedWithMetadata, ICollection<ModuleProviderType>> _getPossibleProviders;

    public ModuleInfoExtendedWithMetadata ModuleInfoExtended { get; }

    public int Index { get; set; }

    [BUTRDataSourceProperty]
    public string Name => ModuleInfoExtended.Name;

    [BUTRDataSourceProperty]
    public string VersionText => ModuleInfoExtended.Version.ToString();

    [BUTRDataSourceProperty]
    public bool IsOfficial => ModuleInfoExtended.IsOfficial;

    [BUTRDataSourceProperty]
    public bool IsDangerous { get => _isDangerous; set => SetField(ref _isDangerous, value); }
    private bool _isDangerous;

    [BUTRDataSourceProperty]
    public LauncherHintVM? DangerousHint { get => _dangerousHint; set => SetField(ref _dangerousHint, value); }
    private LauncherHintVM? _dangerousHint;

    [BUTRDataSourceProperty]
    public LauncherHintVM? DependencyHint { get => _dependencyHint; set => SetField(ref _dependencyHint, value); }
    private LauncherHintVM? _dependencyHint;

    [BUTRDataSourceProperty]
    public bool AnyDependencyAvailable { get => _anyDependencyAvailable; set => SetField(ref _anyDependencyAvailable, value); }
    private bool _anyDependencyAvailable;

    [BUTRDataSourceProperty]
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
    private bool _isSelected;

    [BUTRDataSourceProperty]
    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            if (value != _isDisabled)
            {
                _isDisabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotSelectable));
            }
        }
    }
    private bool _isDisabled;

    [BUTRDataSourceProperty]
    public bool IsExpanded { get => _isExpanded; set => SetField(ref _isExpanded, value); }
    private bool _isExpanded;

    [BUTRDataSourceProperty]
    public string IssuesText
    {
        get => _issuesText;
        set
        {
            if (value != _issuesText)
            {
                _issuesText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotSelectable));
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }
    private string _issuesText = string.Empty;


    [BUTRDataSourceProperty]
    public bool IsNotSelectable => !IsValid || IsDisabled;

    [BUTRDataSourceProperty]
    public bool IsValid => string.IsNullOrWhiteSpace(IssuesText);

    [BUTRDataSourceProperty]
    public bool IsVisible { get => _isVisible; set => SetField(ref _isVisible, value); }
    private bool _isVisible = true;


    [BUTRDataSourceProperty]
    public bool UpdateInfoAvailable { get => _updateInfoAvailable; set => SetField(ref _updateInfoAvailable, value); }
    private bool _updateInfoAvailable;

    [BUTRDataSourceProperty]
    public double CompatibilityScore { get => _compatibilityScore; set => SetField(ref _compatibilityScore, value); }
    private double _compatibilityScore = 1d;

    [BUTRDataSourceProperty]
    public string RecommendedVersion { get => _recommendedVersion; set => SetField(ref _recommendedVersion, value); }
    private string _recommendedVersion = string.Empty;

    [BUTRDataSourceProperty]
    public LauncherHintVM? UpdateHint { get => _updateHint; set => SetField(ref _updateHint, value); }
    private LauncherHintVM? _updateHint;

    public BUTRLauncherModuleVM(ModuleInfoExtendedWithMetadata moduleInfoExtended, Action<BUTRLauncherModuleVM> select, Func<BUTRLauncherModuleVM, IEnumerable<string>> validate,
        Func<ModuleInfoExtendedWithMetadata, ICollection<ModuleProviderType>> getPossibleProviders)
    {
        ModuleInfoExtended = moduleInfoExtended;
        _select = select;
        _validate = validate;
        _getPossibleProviders = getPossibleProviders;

        if (ModuleDependencyConstructor.GetDependencyHint(moduleInfoExtended) is { } str)
        {
            DependencyHint = new LauncherHintVM(str);
            AnyDependencyAvailable = !string.IsNullOrEmpty(str);
        }

        Refresh();
    }

    public void Validate()
    {
        var validationIssues = _validate(this).ToList();

        IssuesText = validationIssues.Count > 0
            ? string.Join("\n", validationIssues)
            : string.Empty;
    }

    public void Refresh()
    {
        var dangerous = string.Empty;
        if (_getPossibleProviders(ModuleInfoExtended) is { } providers && providers.Any(x => x == ModuleProviderType.Steam))
        {
            dangerous += new BUTRTextObject("{=kfMQEOFS}The Module is installed in the game's /Modules folder and on Steam Workshop!{NL}The /Modules version will be used!").ToString();
        }
        if (ModuleChecker.IsObfuscated(ModuleInfoExtended))
        {
            if (dangerous.Length != 0) dangerous += "\n";
            dangerous += new BUTRTextObject("{=aAYdk1zd}The DLL is obfuscated!{NL}There is no guarantee that the code is safe!{NL}The BUTR Team warns of consequences arising from running obfuscated code!").ToString();
        }
        if (!string.IsNullOrEmpty(dangerous))
        {
            IsDangerous = true;
            DangerousHint = new LauncherHintVM(dangerous);
        }
        else
        {
            IsDangerous = false;
            DangerousHint = new LauncherHintVM(dangerous);
        }
    }

    [BUTRDataSourceMethod]
    public void ExecuteSelect()
    {
        if (IsNotSelectable)
            return;

        _select(this);
    }

    [BUTRDataSourceMethod]
    public void ExecuteOpen()
    {
        if (Integrations.IsModOrganizer2)
        {
            var explorer = Path.Combine(Integrations.ModOrganizer2Path!, "explorer++", "Explorer++.exe");
            if (!File.Exists(explorer)) return;
            Process.Start(explorer, $"\"{ModuleInfoExtended.Path}\"");
            return;
        }

        if (!Directory.Exists(ModuleInfoExtended.Path)) return;
        Process.Start(ModuleInfoExtended.Path);
    }

    public void SetUpdateInfo(double compatibilityScore, string? recommendedVersion)
    {
        UpdateInfoAvailable = true;
        CompatibilityScore = compatibilityScore;
        RecommendedVersion = recommendedVersion ?? string.Empty;

        var hasRecommendedVersion = !string.IsNullOrEmpty(recommendedVersion);

        UpdateHint = new LauncherHintVM(new BUTRTextObject(hasRecommendedVersion
                ? "{=HdnFwgVB}Based on BUTR analytics:{NL}{NL}Suggesting to update to {RECOMMENDEDVERSION}.{NL}Compatibility Score {SCORE}%{NL}{NL}{RECOMMENDEDVERSION} has a better compatibility for game {GAMEVERSION} rather than {CURRENTVERSION}!"
                : "{=HdnFwgVA}Based on BUTR analytics:{NL}{NL}Update is not requiured.{NL}Compatibility Score {SCORE}%{NL}{NL}{CURRENTVERSION} is one of the best version for game {GAMEVERSION}")
            .SetTextVariable("SCORE", CompatibilityScore.ToString(CultureInfo.InvariantCulture))
            .SetTextVariable("CURRENTVERSION", VersionText)
            .SetTextVariable("RECOMMENDEDVERSION", RecommendedVersion)
            .SetTextVariable("GAMEVERSION", ApplicationVersionHelper.GameVersionStr()).ToString());
    }

    public void RemoveUpdateInfo()
    {
        UpdateInfoAvailable = false;
        CompatibilityScore = 0d;
        RecommendedVersion = string.Empty;
        UpdateHint = null;
    }


    public override string ToString() => $"{ModuleInfoExtended}, IsSelected: {IsSelected}, IsValid: {IsValid}";
}