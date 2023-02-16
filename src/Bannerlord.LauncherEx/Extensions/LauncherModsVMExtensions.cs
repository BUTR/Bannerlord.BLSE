using Bannerlord.LauncherEx.Mixins;
using Bannerlord.LauncherEx.ViewModels;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Extensions
{
    internal static class LauncherModsVMExtensions
    {
        public static MBBindingList<BUTRLauncherModuleVM>? GetModules(this LauncherModsVM viewModel) =>
            viewModel.GetPropertyValue(nameof(LauncherModsVMMixin.Modules2)) as MBBindingList<BUTRLauncherModuleVM>;
    }
}