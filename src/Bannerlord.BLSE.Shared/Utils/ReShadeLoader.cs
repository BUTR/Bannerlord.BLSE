using System.Diagnostics;
using System.IO;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class ReShadeLoader
{
    public static void LoadReShadeIfNeeded()
    {
        var reShade = new FileInfo("dxgi.dll");
        if (!reShade.Exists) return;

        var reShadeVersionInfo = FileVersionInfo.GetVersionInfo(reShade.FullName);
        var isReShade = reShadeVersionInfo is { CompanyName: "crosire", ProductName: "ReShade" };
        if (!isReShade) return;

        PInvoke.LoadLibrary(reShade.FullName);
    }
}