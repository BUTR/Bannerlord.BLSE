using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherManager.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx;

partial class BUTRLauncherManagerHandler
{
    public async Task SetGameParametersLoadOrderAsync(IEnumerable<IModuleViewModel> modules)
    {
        var loadOrder = GetFromViewModel(modules);
        await SetGameParameterLoadOrderAsync(loadOrder);
        await RefreshGameParametersAsync();
        SaveTWLoadOrder(loadOrder);
    }


    public override Task<string> GetGameVersionAsync() => Task.FromResult(ApplicationVersionHelper.GameVersionStr());

    public override Task<int> GetChangesetAsync() => Task.FromResult(typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0);


    public Task<bool> ShowWarningAsync(string title, string contentPrimary, string contentSecondary)
    {
        return base.ShowWarningAsync(title, contentPrimary, contentSecondary);
    }

    public Task<string> ShowFileOpenAsync(string title, IReadOnlyList<DialogFileFilter> filters)
    {
        return base.ShowFileOpenAsync(title, filters);
    }

    public Task<string> ShowFileSaveAsync(string title, string fileName, IReadOnlyList<DialogFileFilter> filters)
    {
        return base.ShowFileSaveAsync(title, fileName, filters);
    }

    public LoadOrder LoadLoadOrder() => LoadTWLoadOrder();

    public new Task<IReadOnlyList<ModuleInfoExtendedWithMetadata>> GetAllModulesAsync() => base.GetAllModulesAsync();
}