using Bannerlord.LauncherManager.External;

using System;
using System.IO;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class GameInfoProviderImpl : IGameInfoProvider
{
    public static readonly GameInfoProviderImpl Instance = new();

    private readonly string _installPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "../", "../"));

    public string GetInstallPath() => _installPath;
}