using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.LauncherEx.Adapters;
using Bannerlord.LauncherEx.Extensions;
using Bannerlord.LauncherManager;
using Bannerlord.LauncherManager.External;
using Bannerlord.LauncherManager.External.UI;
using Bannerlord.LauncherManager.Models;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using Nito.AsyncEx;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx;

internal partial class BUTRLauncherManagerHandler : LauncherManagerHandler
{
    private static readonly Harmony _harmony = new("Bannerlord.LauncherEx.launchermanager");
    public static BUTRLauncherManagerHandler Default = default!;

    public static void Initialize(UserDataManager userDataManager) => Default = new BUTRLauncherManagerHandler(userDataManager);

    public new Dictionary<string, ModuleInfoExtended> ExtendedModuleInfoCache => base.ExtendedModuleInfoCache;


    private string _executable = Constants.BannerlordExecutable;
    private string _executableParameters = string.Empty;

    private Func<LauncherState>? _getState;
    private Func<IEnumerable<IModuleViewModel>>? _getAllModuleViewModels;
    private Func<IEnumerable<IModuleViewModel>>? _getModuleViewModels;
    private Action<IEnumerable<IModuleViewModel>>? _setModuleViewModels;

    private readonly UserDataManager _userDataManager;

    private BUTRLauncherManagerHandler(UserDataManager userDataManager)
    {
        _userDataManager = userDataManager;

        _harmony.Patch(
            AccessTools2.DeclaredPropertyGetter(typeof(LauncherUI), "AdditionalArgs"),
            postfix: new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(BUTRLauncherManagerHandler), nameof(AdditionalArgsPostfix)), priority: 10000));

        // Note: the new LauncherManager removed loadOrderPersistenceProvider — load/save of the
        // user's saved order is now fully owned by BLSE. SaveTWLoadOrder is called directly from
        // SetGameParametersLoadOrder, and LoadTWLoadOrder is exposed via the LoadLoadOrder()
        // wrapper for the mixins to call at startup.
        Initialize(
            dialogProvider: DialogProviderImpl.Instance,
            notificationProvider: NotificationProviderImpl.Instance,
            fileSystemProvider: FileSystemProviderImpl.Instance,
            gameInfoProvider: GameInfoProviderImpl.Instance,
            launcherStateProvider: new CallbackLauncherStateProvider(
                setGameParameters: (executable, parameters, complete) => { SetGameParametersCallback(executable, parameters); complete(); },
                getOptions: callback => callback(GetTWOptions()),
                getState: callback => callback(GetStateCallback())
            ),
            loadOrderStateProvider: new CallbackLoadOrderStateProvider(
                getAllModuleViewModels: callback => callback(GetAllModuleViewModelsCallback()),
                getModuleViewModels: callback => callback(GetModuleViewModelsCallback()),
                setModuleViewModels: (vms, complete) => { SetModuleViewModelsCallback(vms); complete(); }
            ));

        SetGameStore(LauncherPlatform.PlatformType switch
        {
            LauncherPlatformType.Steam => GameStore.Steam,
            LauncherPlatformType.Epic => GameStore.Epic,
            LauncherPlatformType.Gog => GameStore.GOG,
            LauncherPlatformType.Gdk => GameStore.Xbox,
            _ => GameStore.Unknown
        });
    }
    private static void AdditionalArgsPostfix(ref string __result)
    {
        __result = Default._executableParameters;
    }

    public void RegisterStateProvider(Func<LauncherState> getState)
    {
        _getState = getState;
    }

    public void RegisterModuleViewModelProvider(Func<IEnumerable<IModuleViewModel>> getAllModuleViewModels, Func<IEnumerable<IModuleViewModel>> getModuleViewModels, Action<IEnumerable<IModuleViewModel>> setModuleViewModels)
    {
        _getAllModuleViewModels = getAllModuleViewModels;
        _getModuleViewModels = getModuleViewModels;
        _setModuleViewModels = setModuleViewModels;
    }

    internal LoadOrder LoadTWLoadOrder()
    {
        var state = _getState?.Invoke() ?? LauncherState.Empty;

        var userGameTypeData = state.IsSingleplayer ? _userDataManager.UserData.SingleplayerData : _userDataManager.UserData.MultiplayerData;
        return new LoadOrder(userGameTypeData.ModDatas.DistinctBy(x => x.Id).Select((x, i) => new LoadOrderEntry(x.Id, string.Empty, x.IsSelected, false, i)));
    }

    internal void SaveTWLoadOrder(LoadOrder loadOrder)
    {
        if (_getState is null) return;

        var state = _getState();
        var userGameTypeData = state.IsSingleplayer ? _userDataManager.UserData.SingleplayerData : _userDataManager.UserData.MultiplayerData;

        // Preserve entries for modules that were saved previously but weren't discovered this
        // session (e.g., Steam Workshop offline, mod folder temporarily unavailable). Without
        // this they get permanently dropped from the saved order and reappear at the end of the
        // list on next launch — the "load order keeps resetting" symptom.
        var newIds = new HashSet<string>(loadOrder.Select(x => x.Key));
        var ghosts = userGameTypeData.ModDatas
            .Where(x => !newIds.Contains(x.Id))
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        userGameTypeData.ModDatas.Clear();
        foreach (var (id, entry) in loadOrder)
        {
            userGameTypeData.ModDatas.Add(new UserModData { Id = id, IsSelected = entry.IsSelected, });
        }
        foreach (var ghost in ghosts)
        {
            userGameTypeData.ModDatas.Add(ghost);
        }

        _userDataManager.UserData.GameType = state.IsSingleplayer ? GameType.Singleplayer : GameType.Multiplayer;
        _userDataManager.SaveUserData();
    }

    public LauncherOptions GetTWOptions() => new()
    {
        BetaSorting = LauncherSettings.BetaSorting,
    };

    public bool TryOrderByLoadOrderTW(IEnumerable<string> loadOrder, Func<string, bool> isModuleSelected, [NotNullWhen(false)] out IReadOnlyList<string>? issues,
        out IReadOnlyList<IModuleViewModel> orderedModules, bool overwriteWhenFailure = false)
    {
        // Sync facade over the new async TryOrderByLoadOrderTWAsync. Called from synchronous UI
        // mixin code; AsyncContext.Run pumps continuations on a dedicated single-threaded context,
        // avoiding deadlocks if the async path ever introduces real I/O or Task.Delay-style waits.
        var result = AsyncContext.Run(() => base.TryOrderByLoadOrderTWAsync(loadOrder, isModuleSelected, overwriteWhenFailure));
        issues = result.Result ? null : result.Issues ?? Array.Empty<string>();
        orderedModules = result.OrderedModules;
        return result.Result;
    }
}
