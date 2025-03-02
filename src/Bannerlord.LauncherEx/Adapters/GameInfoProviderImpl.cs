using Bannerlord.LauncherManager.External;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class GameInfoProviderImpl : IGameInfoProvider
{
    public static readonly GameInfoProviderImpl Instance = new();

    private readonly string _installPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory!, "../", "../"));

    public Task<string> GetInstallPathAsync() => Task.FromResult(_installPath);
}