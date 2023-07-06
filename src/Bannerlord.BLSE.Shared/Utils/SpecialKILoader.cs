using System.Diagnostics;
using System.IO;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class SpecialKILoader
{
    public static void LoadSpecialKIfNeeded()
    {
        var specialK = new FileInfo("SpecialK64.dll");
        if (specialK.Exists)
        {
            var dxgiVersionInfo = FileVersionInfo.GetVersionInfo(specialK.FullName);
            var isSpecialK = dxgiVersionInfo is { CompanyName: "The Special☆K Group", ProductName: "Special K" };
            if (!isSpecialK) return;

            PInvoke.LoadLibrary(specialK.FullName);
            return;
        }

        /*
        var dxgi = new FileInfo("dxgi.dll");
        if (dxgi.Exists)
        {
            var dxgiVersionInfo = FileVersionInfo.GetVersionInfo(dxgi.FullName);
            var isSpecialK = dxgiVersionInfo is {CompanyName: "The Special☆K Group", ProductName: "Special K"};
            if (!isSpecialK) return;

            using var key = RegistryHandle.GetHKCUSubkey("Software\\Kaldaien\\Special K");
            var specialKPath = key?.GetStringValue("Path");
            if (string.IsNullOrEmpty(specialKPath)) return;
            var library = Path.Combine(specialKPath, "SpecialK64.dll");
            if (!File.Exists(library)) return;

            PInvoke.LoadLibrary(library);
        }
        */
    }
}