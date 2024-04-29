using Bannerlord.LauncherEx.Extensions;

using System.Diagnostics;
using System.IO;

namespace Bannerlord.LauncherEx.Helpers;

public static class Integrations
{
    public static bool IsModOrganizer2 { get; }
    public static string? ModOrganizer2Path { get; }

    static Integrations()
    {
        if (Process.GetCurrentProcess().ParentProcess() is { MainModule.FileVersionInfo.OriginalFilename: "ModOrganizer.exe", MainModule.FileName: { } path })
        {
            IsModOrganizer2 = true;
            ModOrganizer2Path = Path.GetDirectoryName(path);
        }
    }
}