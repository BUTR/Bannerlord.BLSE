using System.Diagnostics;
using System.IO;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class SpecialKILoader
{
    private static FreeLibrarySafeHandle? _specialK;

    public static void LoadSpecialKIfNeeded()
    {
        var specialK = new FileInfo("SpecialK64.dll");
        if (specialK.Exists)
        {
            var dxgiVersionInfo = FileVersionInfo.GetVersionInfo(specialK.FullName);
            var isSpecialK = dxgiVersionInfo is { CompanyName: "The Special☆K Group", ProductName: "Special K" };
            if (!isSpecialK) return;

            _specialK = PInvoke.LoadLibrary(specialK.FullName);
        }
    }
}