using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.Options;
using Bannerlord.LauncherManager.Localization;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.ViewModels
{
    internal enum OptionsType
    {
        Launcher, Game, Engine
    }

    internal sealed class BUTRLauncherOptionsVM : BUTRViewModel
    {
        private readonly BUTRLauncherManagerHandler _launcherManagerHandler = BUTRLauncherManagerHandler.Default;
        private readonly OptionsType _optionsType;
        private LauncherExData? _launcherExData;
        private readonly Action _saveUserData;
        private readonly Action _refreshOptions;

        [BUTRDataSourceProperty]
        public bool IsDisabled { get => _isDisabled; set => SetField(ref _isDisabled, value); }
        private bool _isDisabled;

        [BUTRDataSourceProperty]
        public MBBindingList<SettingsPropertyVM> SettingProperties { get => _settingProperties; set => SetField(ref _settingProperties, value); }
        private MBBindingList<SettingsPropertyVM> _settingProperties = new();


        public BUTRLauncherOptionsVM(OptionsType optionsType, Action saveUserData, Action refreshOptions)
        {
            _optionsType = optionsType;
            _saveUserData = saveUserData;
            _refreshOptions = refreshOptions;
        }

        public void Refresh()
        {
            SettingProperties.Clear();
            switch (_optionsType)
            {
                case OptionsType.Launcher:
                    RefreshLauncherOptions();
                    break;
                case OptionsType.Game:
                    RefreshGameOptions();
                    break;
                case OptionsType.Engine:
                    RefreshEngineOptions();
                    break;
            }
        }
        private void RefreshLauncherOptions()
        {
            _launcherExData = new LauncherExData(
                LauncherSettings.AutomaticallyCheckForUpdates,
                LauncherSettings.UnblockFiles,
                LauncherSettings.FixCommonIssues,
                LauncherSettings.CompactModuleList,
                LauncherSettings.HideRandomImage,
                LauncherSettings.DisableBinaryCheck,
                LauncherSettings.BetaSorting,
                LauncherSettings.BigMode);

            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=zHo3tzQT}Unblock Files on Start").ToString(),
                HintText = new BUTRTextObject("{=sOInYH9V}Automatically unblock's .dll files on start").ToString(),
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.UnblockFiles))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=LXlsSS8t}Fix Common Issues").ToString(),
                HintText = new BUTRTextObject("{=J9VbkLW4}Fixes issues like 0Harmony.dll being in the /bin folder").ToString(),
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.FixCommonIssues))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=vUAqDj9H}Compact Module List").ToString(),
                HintText = $"{new BUTRTextObject("{=44qrhQ6g}Requires restart!")} {new BUTRTextObject("{=Qn1aPNQM}Makes the Mods tab content smaller")}",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.CompactModuleList))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=GUWbD65T}Disable Binary Compatibility Check").ToString(),
                HintText = $"{new BUTRTextObject("{=z9WqFewN}DISABLED!")} {new BUTRTextObject("{=44qrhQ6g}Requires restart!")} {new BUTRTextObject("{=lmpQeQBS}Disables Launcher's own check for binary compatibility of mods")}",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.DisableBinaryCheck))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=iD27wEq7}Hide Random Image").ToString(),
                HintText = new BUTRTextObject("{=LaPvZjwC}Hide's the Rider image so the launcher looks more compact").ToString(),
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.HideRandomImage))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=JT7QnJJA}Unblock Files").ToString(),
                HintText = new BUTRTextObject("{=VMIp4503}Unblock all .dll files on /Modules folder").ToString(),
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => new BUTRTextObject("{=RDLKkiVk}Unblock").ToString(), _ =>
                {
                    NtfsUnblocker.UnblockPath(Path.Combine(BasePath.Name, ModuleInfoHelper.ModulesFolder));
                })
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=QJSBiZdJ}Beta Sorting").ToString(),
                HintText = new BUTRTextObject("{=HVhaqeb4}Uses the new sorting algorithm after v1.12.x. Disable to use the old algorithm").ToString(),
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.BetaSorting))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=1zt99vTt}Big Mode").ToString(),
                HintText = new BUTRTextObject("{=XUSDSpvf}Makes the launcher bigger in height").ToString(),
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(LauncherSettings).GetProperty(nameof(LauncherSettings.BigMode))!, this)
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=IsR2rbnG}Restore Game Options Backup").ToString(),
                HintText = new BUTRTextObject("{=uKUsA3Sp}BUTRLoader always makes a backup before saving the first time. This will restore the original files").ToString(),
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => new BUTRTextObject("{=TLDgPay9}Restore").ToString(), _ =>
                {
                    var backupPath = $"{ConfigReader.GameConfigPath}.bak";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, ConfigReader.GameConfigPath, true);
                        File.Delete(backupPath);
                        _refreshOptions();
                    }
                })
            }));
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = new BUTRTextObject("{=5XzSM7RN}Restore Engine Options Backup").ToString(),
                HintText = new BUTRTextObject("{=uKUsA3Sp}BUTRLoader always makes a backup before saving the first time. This will restore the original files").ToString(),
                SettingType = SettingType.Button,
                PropertyReference = new ProxyRef<string>(() => new BUTRTextObject("{=TLDgPay9}Restore").ToString(), _ =>
                {
                    var backupPath = $"{ConfigReader.EngineConfigPath}.bak";
                    if (File.Exists(backupPath))
                    {
                        File.Copy(backupPath, ConfigReader.EngineConfigPath, true);
                        File.Delete(backupPath);
                        _refreshOptions();
                    }
                })
            }));
            /*
            SettingProperties.Add(new SettingsPropertyVM(new SettingsPropertyDefinition
            {
                DisplayName = "Automatically Check for Updates",
                SettingType = SettingType.Bool,
                PropertyReference = new PropertyRef(typeof(BUTRLoaderAppDomainManager).GetProperty(nameof(BUTRLoaderAppDomainManager.AutomaticallyCheckForUpdates))!, this)
            }));
            */
        }
        private void RefreshGameOptions()
        {
            try
            {
                foreach (var (key, value) in ConfigReader.GetGameOptions(path => File.Exists(path) ? File.ReadAllBytes(path) : null))
                {
                    SettingProperties.Add(CreateSettingsPropertyVM(key, value, ToSeparateWords));
                }
            }
            catch (Exception) { /* ignore */ }
        }
        private void RefreshEngineOptions()
        {
            try
            {
                foreach (var (key, value) in ConfigReader.GetEngineOptions(path => File.Exists(path) ? File.ReadAllBytes(path) : null))
                {
                    SettingProperties.Add(CreateSettingsPropertyVM(key, value, x => ToTitleCase(x.Replace("_", " "))));
                }
            }
            catch (Exception) { /* ignore */ }
        }

        public void Save()
        {
            switch (_optionsType)
            {
                case OptionsType.Launcher:
                    SaveLauncherOptions();
                    break;
                case OptionsType.Game:
                    SaveGameOptions();
                    break;
                case OptionsType.Engine:
                    SaveEngineOptions();
                    break;
            }
        }
        private void SaveLauncherOptions()
        {
            if (_launcherExData is null)
                return;

            if (_launcherExData.AutomaticallyCheckForUpdates != LauncherSettings.AutomaticallyCheckForUpdates)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.UnblockFiles != LauncherSettings.UnblockFiles)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.FixCommonIssues != LauncherSettings.FixCommonIssues)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.CompactModuleList != LauncherSettings.CompactModuleList)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.HideRandomImage != LauncherSettings.HideRandomImage)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.DisableBinaryCheck != LauncherSettings.DisableBinaryCheck)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.BetaSorting != LauncherSettings.BetaSorting)
            {
                _saveUserData();
                return;
            }

            if (_launcherExData.BigMode != LauncherSettings.BigMode)
            {
                _saveUserData();
                return;
            }
        }
        private void SaveGameOptions()
        {
            var backupPath = $"{ConfigReader.GameConfigPath}.bak";
            if (File.Exists(ConfigReader.GameConfigPath) && !File.Exists(backupPath))
                File.Copy(ConfigReader.GameConfigPath, backupPath);

            var hasChanges = false;
            var sb = new StringBuilder();
            foreach (var settingProperty in SettingProperties)
            {
                if (settingProperty.SettingPropertyDefinition is not ConfigSettingsPropertyDefinition propertyDefinition)
                    continue;

                if (!string.Equals(propertyDefinition.OriginalValue, settingProperty.ValueAsString, StringComparison.Ordinal))
                    hasChanges = true;

                sb.AppendLine($"{propertyDefinition.ConfigKey}={settingProperty.ValueAsString}");
            }
            if (hasChanges)
            {
                File.WriteAllText(ConfigReader.GameConfigPath, sb.ToString());
                var options = _launcherManagerHandler.GetTWOptions();
                BUTRLocalizationManager.ActiveLanguage = options.Language;
            }
        }
        private void SaveEngineOptions()
        {
            var backupPath = $"{ConfigReader.EngineConfigPath}.bak";
            if (File.Exists(ConfigReader.EngineConfigPath) && !File.Exists(backupPath))
                File.Copy(ConfigReader.EngineConfigPath, backupPath);

            var hasChanges = false;
            var sb = new StringBuilder();
            foreach (var settingProperty in SettingProperties)
            {
                if (settingProperty.SettingPropertyDefinition is not ConfigSettingsPropertyDefinition propertyDefinition)
                    continue;

                if (!string.Equals(propertyDefinition.OriginalValue, settingProperty.ValueAsString, StringComparison.Ordinal))
                    hasChanges = true;

                sb.AppendLine($"{propertyDefinition.ConfigKey} = {settingProperty.ValueAsString}");
            }
            if (hasChanges)
                File.WriteAllText(ConfigReader.EngineConfigPath, sb.ToString());
        }

        private static SettingsPropertyVM CreateSettingsPropertyVM(string key, string value, Func<string, string> keyProcessor)
        {
            var settingsType = bool.TryParse(value, out _) ? SettingType.Bool
                : int.TryParse(value, out _) ? SettingType.Int
                : float.TryParse(value, out _) ? SettingType.Float
                : SettingType.String;
            var storage = settingsType switch
            {
                SettingType.Bool => (IRef) new StorageRef<bool>(bool.Parse(value)),
                SettingType.Int => (IRef) new StorageRef<int>(int.Parse(value)),
                SettingType.Float => (IRef) new StorageRef<float>(float.Parse(value)),
                SettingType.String => (IRef) new StorageRef<string>(value),
                _ => throw new ArgumentOutOfRangeException()
            };
            var propertyRef = settingsType switch
            {
                SettingType.Bool => (IRef) new ProxyRef<bool>(() => (bool) storage.Value!, val => { storage.Value = val; }),
                SettingType.Int => (IRef) new ProxyRef<int>(() => (int) storage.Value!, val => { storage.Value = val; }),
                SettingType.Float => (IRef) new ProxyRef<float>(() => (float) storage.Value!, val => { storage.Value = val; }),
                SettingType.String => (IRef) new ProxyRef<string>(() => (string) storage.Value!, val => { storage.Value = val; }),
                _ => throw new ArgumentOutOfRangeException()
            };
            return new SettingsPropertyVM(new ConfigSettingsPropertyDefinition
            {
                ConfigKey = key,
                OriginalValue = value,
                DisplayName = keyProcessor(key),
                SettingType = settingsType,
                PropertyReference = propertyRef
            });
        }

        [return: NotNullIfNotNull("value")]
        private static string? ToSeparateWords(string? value)
        {
            if (value == null) return null;
            if (value.Length <= 1) return value;

            var inChars = value.ToCharArray();
            var uCWithAnyLC = new List<int>();
            var i = 0;
            while (i < inChars.Length && char.IsUpper(inChars[i])) { ++i; }

            for (; i < inChars.Length; i++)
            {
                if (!char.IsUpper(inChars[i])) continue;
                uCWithAnyLC.Add(i);
                if (++i >= inChars.Length || !char.IsUpper(inChars[i])) continue;
                while (++i < inChars.Length)
                {
                    if (char.IsUpper(inChars[i])) continue;
                    uCWithAnyLC.Add(i - 1);
                    break;
                }
            }

            var outChars = new char[inChars.Length + uCWithAnyLC.Count];
            var lastIndex = 0;
            for (i = 0; i < uCWithAnyLC.Count; i++)
            {
                var currentIndex = uCWithAnyLC[i];
                Array.Copy(inChars, lastIndex, outChars, lastIndex + i, currentIndex - lastIndex);
                outChars[currentIndex + i] = ' ';
                lastIndex = currentIndex;
            }

            var lastPos = lastIndex + uCWithAnyLC.Count;
            Array.Copy(inChars, lastIndex, outChars, lastPos, outChars.Length - lastPos);
            return new string(outChars);
        }

        [return: NotNullIfNotNull("value")]
        private static string? ToTitleCase(string? value) => value is null ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);
    }
}