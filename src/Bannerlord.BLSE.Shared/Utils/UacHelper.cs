using Microsoft.Win32.SafeHandles;

using System;
using System.Diagnostics;
using System.Linq;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;

namespace Bannerlord.BLSE.Shared.Utils;

public static class UacHelper
{
    private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
    private const string uacRegistryValue = "EnableLUA";

    private static uint STANDARD_RIGHTS_READ = 0x00020000;
    private static uint TOKEN_QUERY = 0x0008;

    public enum TOKEN_ELEVATION_TYPE
    {
        TokenElevationTypeDefault = 1,
        TokenElevationTypeFull,
        TokenElevationTypeLimited
    }

    public static bool IsUacEnabled
    {
        get
        {
            using var key = RegistryHandle.GetHKLMSubkey(uacRegistryKey);
            return key?.GetDwordValue(uacRegistryValue)?.Equals(1) == true;
        }
    }

    public static unsafe bool? IsProcessElevated(SafeProcessHandle processHandle)
    {
        try
        {
            if (IsUacEnabled)
            {
                if (!PInvoke.OpenProcessToken(processHandle, TOKEN_ACCESS_MASK.TOKEN_READ, out var tokenHandle) || tokenHandle.IsClosed || tokenHandle.IsInvalid)
                    return null;
                using var __ = tokenHandle;

                var elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;
                const uint elevationResultSize = sizeof(TOKEN_ELEVATION_TYPE);
                uint result = 0;
                if (PInvoke.GetTokenInformation((HANDLE) tokenHandle.DangerousGetHandle(), TOKEN_INFORMATION_CLASS.TokenElevationType, &elevationResult, elevationResultSize, &result))
                    return elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;

                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }

        return false;
    }

    public static void CheckSteam()
    {
        var thisProcess = Process.GetCurrentProcess();

        var steamProcesses = Process.GetProcessesByName("steam");
        if (steamProcesses.Length != 1) return;
        var steamProcess = steamProcesses.First();

        using var steamProcessHandle = steamProcess.SafeHandle;
        var steamElevated = IsProcessElevated(steamProcessHandle);
        if (steamElevated is null) return;

        using var thisProcessHandle = thisProcess.SafeHandle;
        var thisElevated = IsProcessElevated(thisProcessHandle);
        if (thisElevated is null) return;

        if (steamElevated == true && thisElevated != true)
        {
            MessageBoxDialog.Show(@"Steam is launched as Admin, but BLSE is not!
The game won't work if Steam has higher privileges than the game!
Please run Steam as a user or run the game as Admin!", "Error from BLSE!", MessageBoxButtons.Ok, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
}