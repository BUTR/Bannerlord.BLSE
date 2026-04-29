using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.LauncherManager.Models;

using Nito.AsyncEx;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx;

partial class BUTRLauncherManagerHandler
{
    public void SetGameParametersLoadOrder(IEnumerable<IModuleViewModel> modules) => SaveTWLoadOrder(new LoadOrder(modules));


    public override Task<string> GetGameVersionAsync() => Task.FromResult(ApplicationVersionHelper.GameVersionStr());

    public override Task<int> GetChangesetAsync() =>
        Task.FromResult(typeof(TaleWorlds.Library.ApplicationVersion).GetField("DefaultChangeSet")?.GetValue(null) as int? ?? 0);

    public LoadOrder LoadLoadOrder() => LoadTWLoadOrder();

    public IReadOnlyList<ModuleInfoExtendedWithMetadata> GetAllModules() => AsyncContext.Run(() => GetAllModulesAsync());
}
