using System.IO;

using Windows.Win32;

namespace Bannerlord.LauncherEx.Helpers
{
    /// <summary>
    /// https://stackoverflow.com/a/21266072
    /// </summary>
    internal static class NtfsUnblocker
    {
        public static void UnblockPath(string path, string wildcard = "*")
        {
            foreach (var file in Directory.GetFiles(path, wildcard))
                UnblockFile(file);

            foreach (var dir in Directory.GetDirectories(path))
                UnblockPath(dir);
        }

        public static bool UnblockFile(string fileName) => PInvoke.DeleteFile($"{fileName}:Zone.Identifier");
    }
}