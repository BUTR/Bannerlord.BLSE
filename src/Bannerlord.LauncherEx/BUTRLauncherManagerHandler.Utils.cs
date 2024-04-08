using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherManager.Models;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx;

partial class BUTRLauncherManagerHandler
{
    public void SetGameParametersLoadOrder(IEnumerable<IModuleViewModel> modules) => SaveLoadOrder(GetFromViewModel(modules));


    public override string GetGameVersion() => ApplicationVersionHelper.GameVersionStr();

    public override int GetChangeset() => typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0;

    protected override IEnumerable<ModuleInfoExtendedWithPath> ReloadModules() => ModuleInfoHelper.GetModules().Select(x => new ModuleInfoExtendedWithPath(x, x.Path));


    // More of a reminder how the callbacks should be handled if needed in C#
    public Task<bool> ShowWarning(string title, string contentPrimary, string contentSecondary)
    {
        var tcs = new TaskCompletionSource<bool>();
        base.ShowWarning(title, contentPrimary, contentSecondary, tcs.SetResult);
        return tcs.Task;
    }

    public Task<string> ShowFileOpen(string title, IReadOnlyList<DialogFileFilter> filters)
    {
        var tcs = new TaskCompletionSource<string>();
        base.ShowFileOpen(title, filters, tcs.SetResult);
        return tcs.Task;
    }

    public Task<string> ShowFileSave(string title, string fileName, IReadOnlyList<DialogFileFilter> filters)
    {
        var tcs = new TaskCompletionSource<string>();
        base.ShowFileSave(title, fileName, filters, tcs.SetResult);
        return tcs.Task;
    }

    public new LoadOrder LoadLoadOrder() => base.LoadLoadOrder();
}