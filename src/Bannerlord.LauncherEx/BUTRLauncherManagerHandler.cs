using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.LauncherEx.Adapters;
using Bannerlord.LauncherManager;
using Bannerlord.LauncherManager.External;
using Bannerlord.LauncherManager.External.UI;
using Bannerlord.LauncherManager.Models;
using Bannerlord.ModuleManager;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.MountAndBlade.Launcher.Library.UserDatas;

namespace Bannerlord.LauncherEx
{
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

            Initialize(
                dialogProvider: DialogProviderImpl.Instance,
                notificationProvider: NotificationProviderImpl.Instance,
                fileSystemProvider: FileSystemProviderImpl.Instance,
                gameInfoProvider: GameInfoProviderImpl.Instance,
                loadOrderPersistenceProvider: new CallbackLoadOrderPersistenceProvider(
                    loadLoadOrder: LoadTWLoadOrder,
                    saveLoadOrder: SaveTWLoadOrder
                ),
                launcherStateProvider: new CallbackLauncherStateProvider(
                    setGameParameters: SetGameParametersCallback,
                    getOptions: GetTWOptions,
                    getState: GetStateCallback
                ),
                loadOrderStateProvider: new CallbackLoadOrderStateProvider(
                    getAllModuleViewModels: GetAllModuleViewModelsCallback,
                    getModuleViewModels: GetModuleViewModelsCallback,
                    setModuleViewModels: SetModuleViewModelsCallback
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

        private LoadOrder LoadTWLoadOrder()
        {
            var state = _getState?.Invoke() ?? LauncherState.Empty;

            var userGameTypeData = state.IsSingleplayer ? _userDataManager.UserData.SingleplayerData : _userDataManager.UserData.MultiplayerData;
            return new LoadOrder(userGameTypeData.ModDatas.Select((x, i) => new LoadOrderEntry(x.Id, string.Empty, x.IsSelected, false, i)));
        }

        private void SaveTWLoadOrder(LoadOrder loadOrder)
        {
            if (_getState is null) return;

            var state = _getState();
            var userGameTypeData = state.IsSingleplayer ? _userDataManager.UserData.SingleplayerData : _userDataManager.UserData.MultiplayerData;
            userGameTypeData.ModDatas.Clear();
            foreach (var (id, entry) in loadOrder)
            {
                userGameTypeData.ModDatas.Add(new UserModData { Id = id, IsSelected = entry.IsSelected, });
            }

            _userDataManager.UserData.GameType = state.IsSingleplayer ? GameType.Singleplayer : GameType.Multiplayer;
            _userDataManager.SaveUserData();
        }

        public LauncherOptions GetTWOptions() => new()
        {
            BetaSorting = LauncherSettings.BetaSorting,
            FixCommonIssues = LauncherSettings.FixCommonIssues,
            UnblockFiles = true, // TODO: Remove. Always unblock
            Language = Manager.GetActiveLanguage(),
        };

        public new bool TryOrderByLoadOrderTW(IEnumerable<string> loadOrder, Func<string, bool> isModuleSelected, [NotNullWhen(false)] out IReadOnlyList<string>? issues,
            out IReadOnlyList<IModuleViewModel> orderedModules, bool overwriteWhenFailure = false)
            => base.TryOrderByLoadOrderTW(loadOrder, isModuleSelected, out issues, out orderedModules, overwriteWhenFailure);
    }
}