using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Bannerlord.BLSE.Shared.Utils;

public static class Unblocker
{
    private static readonly Harmony _harmony = new("Bannerlord.BLSE.Shared.Patches.Unblocker");
    private static Thread? _currentUnblockingThread;


    public static void Unblock()
    {
        if (_currentUnblockingThread is not null)
            return;

        Assembly.Load(new AssemblyName("TaleWorlds.Starter.Library"));

        var result = _harmony.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            prefix: AccessTools2.Method(typeof(Unblocker), nameof(MainPrefix)));

        if (result)
        {
            _currentUnblockingThread = new Thread(UnblockFiles);
            _currentUnblockingThread.Start();
        }
    }

    private static void MainPrefix()
    {
        // We prevent the game from being started if we didn't finish with unblocking
        try
        {
            _currentUnblockingThread?.Join();
        }
        catch (Exception) { /* ignore */ }

        _harmony.Unpatch(AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"), AccessTools2.Method(typeof(Unblocker), nameof(MainPrefix)));
    }

    private static void UnblockFiles()
    {
        var modulesPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../", "../", ModuleInfoHelper.ModulesFolder));
        if (Directory.Exists(modulesPath))
        {
            try
            {
                NtfsUnblocker.UnblockDirectory(modulesPath, "*.dll");
            }
            catch { /* ignore */ }
        }
    }
}