using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class NtfsUnblocker
{
    public static void UnblockDirectory(string path, string wildcard = "*")
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Parallel.ForEach(Directory.EnumerateFiles(path, wildcard, SearchOption.AllDirectories), UnblockFile);
    }

    public static void UnblockFile(string fileName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            PInvoke.DeleteFile($"{fileName}:Zone.Identifier");
    }
}