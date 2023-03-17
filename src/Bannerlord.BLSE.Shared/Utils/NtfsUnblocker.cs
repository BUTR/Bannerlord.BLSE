using System.IO;
using System.Threading.Tasks;

using Windows.Win32;

namespace Bannerlord.BLSE.Shared.Utils
{
    internal static class NtfsUnblocker
    {
        public static void UnblockDirectory(string path, string wildcard = "*") => Parallel.ForEach(Directory.EnumerateFiles(path, wildcard, SearchOption.AllDirectories), UnblockFile);

        public static void UnblockFile(string fileName) => PInvoke.DeleteFile($"{fileName}:Zone.Identifier");
    }
}