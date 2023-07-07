using System.Diagnostics;
using System.IO;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class ReShadeLoader
{
    private static FreeLibrarySafeHandle? _reShade;

    public static void LoadReShadeIfNeeded()
    {
        var dxgi = new FileInfo("dxgi.dll");
        if (dxgi.Exists)
        {
            var dxgiVersionInfo = FileVersionInfo.GetVersionInfo(dxgi.FullName);
            var isReShade = dxgiVersionInfo is { CompanyName: "crosire", ProductName: "ReShade" };
            if (!isReShade) return;

            _reShade = PInvoke.LoadLibrary(dxgi.FullName);
        }
    }
}