using Microsoft.Win32.SafeHandles;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.ToolHelp;

namespace Bannerlord.LauncherEx.Extensions;

internal static class ProcessExtensions
{
    public static int ParentProcessId(this Process process) => ParentProcessId(process.Id);
    public static Process? ParentProcess(this Process process) => ParentProcessId(process.Id) is var pId && pId is not -1 ? Process.GetProcessById(pId) : null;

    private static int ParentProcessId(int id)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var pe32 = new PROCESSENTRY32
            {
                dwSize = (uint) Marshal.SizeOf(typeof(PROCESSENTRY32))
            };
            using var hSnapshot = new SafeSnapshotHandle(PInvoke.CreateToolhelp32Snapshot(CREATE_TOOLHELP_SNAPSHOT_FLAGS.TH32CS_SNAPPROCESS, (uint) id));
            if (hSnapshot.IsInvalid) return -1;

            if (!PInvoke.Process32First(hSnapshot, ref pe32))
                return -1;

            do
            {
                if (pe32.th32ProcessID == (uint) id)
                    return (int) pe32.th32ParentProcessID;
            } while (PInvoke.Process32Next(hSnapshot, ref pe32));
        }

        return -1;
    }

    [SuppressUnmanagedCodeSecurity]
    private sealed class SafeSnapshotHandle : SafeHandleMinusOneIsInvalid
    {
        private readonly HANDLE _handle;

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeSnapshotHandle(HANDLE handle) : base(true)
        {
            _handle = handle;
            SetHandle(_handle);
        }

        protected override bool ReleaseHandle() => PInvoke.CloseHandle(_handle);
    }
}