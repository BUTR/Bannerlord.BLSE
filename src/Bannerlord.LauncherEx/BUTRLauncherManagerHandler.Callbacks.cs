using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.LauncherEx
{
    partial class BUTRLauncherManagerHandler
    {
        private LauncherState GetStateCallback() => _getState?.Invoke() ?? LauncherState.Empty;

        private IModuleViewModel[]? GetAllModuleViewModelsCallback() => _getAllModuleViewModels?.Invoke()?.ToArray() ?? Array.Empty<IModuleViewModel>();
        private IModuleViewModel[]? GetModuleViewModelsCallback() => _getModuleViewModels?.Invoke()?.ToArray() ?? Array.Empty<IModuleViewModel>();
        private void SetModuleViewModelsCallback(IReadOnlyList<IModuleViewModel> orderedViewModels) => _setModuleViewModels?.Invoke(orderedViewModels);

        private void SetGameParametersCallback(string executable, IReadOnlyList<string> parameters)
        {
            _executable = executable;
            _executableParameters = string.Join(" ", parameters);
        }
    }
}