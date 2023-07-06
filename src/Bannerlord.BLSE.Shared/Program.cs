using Bannerlord.BLSE.Shared.Utils;

using System;
using System.Linq;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Bannerlord.BLSE.Shared;

public static class Program
{
    public static void Main(string[] args)
    {
        //PInvoke.ShowWindow(PInvoke.GetConsoleWindow(), SHOW_WINDOW_CMD.SW_HIDE);

        if (PlatformHelper.IsSteam())
            UacHelper.CheckSteam();

        LauncherExceptionHandler.Watch();

        ReShadeLoader.LoadReShadeIfNeeded();

        switch (args[0])
        {
            case "launcher":
            {
                // Users can opt-out of unblocking for I guess performance reasons?
                if (!args.Contains("/nounblock")) Unblocker.Unblock();
                Launcher.Launch(args.Skip(1).ToArray());
                break;
            }
            case "launcherex":
            {
                // Users can opt-out of unblocking for I guess performance reasons?
                if (!args.Contains("/nounblock")) Unblocker.Unblock();
                LauncherEx.Launch(args.Skip(1).ToArray());
                break;
            }
            case "standalone":
            {
                // Since standalone is for external tools, they might unlock the files themselves
                if (args.Contains("/unblock")) Unblocker.Unblock();
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

    public static int NativeEntry2(string args)
    {
        GetEntryAssembly.Enable();
        Main(args.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries));
        return 0;
    }
}