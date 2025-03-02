using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.LauncherEx;

partial class BUTRLauncherManagerHandler
{
    private void GetStateCallback(Action<LauncherState> callback) => callback(_getState?.Invoke() ?? LauncherState.Empty);

    private void GetAllModuleViewModelsCallback(Action<IModuleViewModel[]?> callback) => callback(_getAllModuleViewModels?.Invoke()?.ToArray() ?? []);
    private void GetModuleViewModelsCallback(Action<IModuleViewModel[]?> callback) => callback(_getModuleViewModels?.Invoke()?.ToArray() ?? []);
    private void SetModuleViewModelsCallback(IReadOnlyList<IModuleViewModel> orderedViewModels, Action callback) => _setModuleViewModels?.Invoke(orderedViewModels);

    private void SetGameParametersCallback(string executable, IReadOnlyList<string> parameters, Action callback)
    {
        _executable = executable;
        _executableParameters = string.Join(" ", parameters);
        callback();
    }
}