using Bannerlord.BLSE.Shared.Utils;
using Bannerlord.BUTR.Shared.Helpers;

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bannerlord.BLSE.Shared;

public static class Program
{
    /*
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    */

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

    public static void Main(string[] args)
    {
        /*
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
        */

        switch (args[0])
        {
            case "launcher":
            {
                UnblockFiles();
                Launcher.Launch(args.Skip(1).ToArray());
                break;
            }
            case "launcherex":
            {
                UnblockFiles();
                LauncherEx.Launch(args.Skip(1).ToArray());
                break;
            }
            case "standalone":
            {
                UnblockFiles();
                Standalone.Launch(args.Skip(1).ToArray());
                break;
            }
        }
    }

    public static void NativeEntry(int argc, IntPtr argv)
    {
        var args = new string[argc];
        for (var i = 0; i < argc; i++, argv += IntPtr.Size)
        {
            var charPtr = Marshal.ReadIntPtr(argv);
            args[i] = Marshal.PtrToStringAnsi(charPtr) ?? string.Empty;
        }
        Main(args);
    }
}