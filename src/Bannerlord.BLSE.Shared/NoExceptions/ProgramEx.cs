using Bannerlord.LauncherEx;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Linq;
using System.Reflection;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;
using TaleWorlds.TwoDimension.Standalone;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace Bannerlord.BLSE.Shared.NoExceptions;

public sealed class ProgramEx
{
    private record LauncherExContext(GraphicsForm GraphicsForm, StandaloneUIDomain StandaloneUIDomain, WindowsFrameworkEx WindowsFramework)
    {
        public static LauncherExContext Create()
        {
            var resourceDepot = new ResourceDepot();
            resourceDepot.AddLocation(BasePath.Name, "Modules/Native/LauncherGUI/");
            resourceDepot.CollectResources();
            resourceDepot.StartWatchingChangesInDepot();

            var graphicsForm = new GraphicsForm(1154, 701, resourceDepot, true, true, true, "M&B II: Bannerlord");
            var standaloneUIDomain = new StandaloneUIDomain(graphicsForm, resourceDepot);
            var windowsFrameworkEx = new WindowsFrameworkEx { ThreadConfig = WindowsFrameworkThreadConfig.NoThread };

            return new LauncherExContext(graphicsForm, standaloneUIDomain, windowsFrameworkEx);
        }

        public void Initialize()
        {
            WindowsFramework.Initialize(new FrameworkDomain[] { StandaloneUIDomain });
            WindowsFramework.RegisterMessageCommunicator(GraphicsForm);
            WindowsFramework.Start();
        }

        public void DeInitialize()
        {
            WindowsFramework.UnRegisterMessageCommunicator(GraphicsForm);
            GraphicsForm.Destroy();
            WindowsFramework.Stop();
        }
    }
    private record StartGameData
    {
        public bool ShouldStart { get; init; }
        public string[] Args { get; init; } = Array.Empty<string>();
    }

    private const string StarterExecutable = "Bannerlord.exe";
    private static StartGameData _gameData = new();
    private static LauncherExContext? _context;

    public static void Main(string[] args)
    {
#if v110
        TaleWorlds.Library.Common.SetInvariantCulture();
#endif
        Common.PlatformFileHelper = new PlatformFileHelperPC("Mount and Blade II Bannerlord");
        Debug.DebugManager = new LauncherDebugManager();
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        new Harmony("Bannerlord.BLSE.Shared.LauncherExEx").TryPatch(
            AccessTools2.Method(typeof(TaleWorlds.MountAndBlade.Launcher.Library.Program), nameof(TaleWorlds.MountAndBlade.Launcher.Library.Program.StartGame)),
            AccessTools2.Method(typeof(ProgramEx), nameof(StartGamePrefix)));

        _gameData = _gameData with { Args = args.ToArray() };

        LauncherPlatform.Initialize();
        _context = LauncherExContext.Create();
        _context.Initialize();
        LauncherPlatform.Destroy();

        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

        if (_gameData.ShouldStart)
            TaleWorlds.Starter.Library.Program.Main(_gameData.Args.ToArray());
    }

    public static bool StartGamePrefix()
    {
        if (_context is null) return true;

        _gameData = _gameData with
        {
            Args = _gameData.Args.Concat(new[] { _context.StandaloneUIDomain.AdditionalArgs }).ToArray(),
            ShouldStart = true,
        };
        AuxFinalize();
        return false;
    }

    private static void AuxFinalize()
    {
        if (_context is not null)
        {
            _context.DeInitialize();
            _context = null;
        }

        var launcherDebugManager = Debug.DebugManager as LauncherDebugManager;
        launcherDebugManager?.OnFinalize();

        User32.SetForegroundWindow(Kernel32.GetConsoleWindow());

        Manager.Disable();
    }

    private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args) => args.Name.Contains("ManagedStarter") ? Assembly.LoadFrom(StarterExecutable) : null;
}