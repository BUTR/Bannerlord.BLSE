using Bannerlord.LauncherEx.Helpers;

using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.LauncherEx.Mixins
{
    internal sealed class LauncherNewsVMMixin : ViewModelMixin<LauncherNewsVMMixin, LauncherNewsVM>
    {
        [BUTRDataSourceProperty]
        public bool IsDisabled2 { get => _isDisabled2; set => SetField(ref _isDisabled2, value); }
        private bool _isDisabled2;

        public LauncherNewsVMMixin(LauncherNewsVM launcherNewsVM) : base(launcherNewsVM) { }
    }
}