using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;
using Bannerlord.LauncherManager.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.ViewModels
{
    internal sealed class BUTRLauncherModuleVM : BUTRViewModel, IModuleViewModel
    {
        private readonly Action<BUTRLauncherModuleVM> _select;
        private readonly Func<BUTRLauncherModuleVM, IEnumerable<string>> _validate;

        public ModuleInfoExtendedWithPath ModuleInfoExtended { get; }

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

        public BUTRLauncherModuleVM(ModuleInfoExtendedWithPath moduleInfoExtended, Action<BUTRLauncherModuleVM> select, Func<BUTRLauncherModuleVM, IEnumerable<string>> validate)
        {
            ModuleInfoExtended = moduleInfoExtended;
            _select = select;
            _validate = validate;

            if (ModuleDependencyConstructor.GetDependencyHint(moduleInfoExtended) is { } str)
            {
                DependencyHint = new LauncherHintVM(str);
                AnyDependencyAvailable = !string.IsNullOrEmpty(str);
            }

            var dangerous = string.Empty;
            if (ModuleChecker.IsInstalledInMainAndExternalModuleDirectory(moduleInfoExtended))
            {
                dangerous += new BUTRTextObject("{=kfMQEOFS}The Module is installed in the game's /Modules folder and on Steam Workshop!{NL}The /Modules version will be used!").ToString();
            }
            if (ModuleChecker.IsObfuscated(moduleInfoExtended))
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

        public void Validate()
        {
            var validationIssues = _validate(this).ToList();

            IssuesText = validationIssues.Count > 0
                ? string.Join("\n", validationIssues)
                : string.Empty;
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

        public override string ToString() => $"{ModuleInfoExtended}, IsSelected: {IsSelected}, IsValid: {IsValid}";
    }
}