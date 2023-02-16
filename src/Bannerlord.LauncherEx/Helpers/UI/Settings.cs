using System.Diagnostics.CodeAnalysis;

namespace Bannerlord.LauncherEx.Helpers
{
    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/SettingType.cs
    /// </summary>
    internal enum SettingType
    {
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        NONE = -1,
        Bool,
        Int,
        Float,
        String,
        Button,
    }

    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/Models/ISettingsPropertyDefinition.cs
    /// </summary>
    internal interface ISettingsPropertyDefinition
    {
        IRef PropertyReference { get; }

        SettingType SettingType { get; }

        string DisplayName { get; }
        string HintText { get; }
        decimal MinValue { get; }
        decimal MaxValue { get; }
        string Content { get; }
    }

    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Settings/Models/SettingsPropertyDefinition.cs
    /// </summary>
    internal class SettingsPropertyDefinition : ISettingsPropertyDefinition
    {
        public string DisplayName { get; init; } = string.Empty;
        public string HintText { get; init; } = string.Empty;
        public IRef PropertyReference { get; init; } = default!;
        public SettingType SettingType { get; init; } = default!;

        public decimal MinValue { get; init; } = default!;
        public decimal MaxValue { get; init; } = default!;
        public string Content { get; init; } = default!;
    }
    internal class ConfigSettingsPropertyDefinition : SettingsPropertyDefinition
    {
        public string ConfigKey { get; init; } = string.Empty;
        public string OriginalValue { get; init; } = string.Empty;
    }
}