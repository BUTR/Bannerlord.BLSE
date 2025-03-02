using Bannerlord.LauncherManager.Models;

namespace Bannerlord.LauncherEx;

public record LauncherExLauncherOptions : LauncherOptions
{
    public bool FixCommonIssues { get; set; }
    public bool UnblockFiles { get; set; }
    public string Language { get; set; }
}