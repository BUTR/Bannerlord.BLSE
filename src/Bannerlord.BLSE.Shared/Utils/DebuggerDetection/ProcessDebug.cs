using System.Diagnostics;

using Windows.Win32;
using Windows.Win32.System.Threading;

namespace Bannerlord.BLSE.Shared.Utils.DebuggerDetection
{
    internal static class ProcessDebug
    {
        private const PROCESSINFOCLASS PROCESS_DEBUG_OBJECT_HANDLE = (PROCESSINFOCLASS) 30;

        /// <summary>
        /// Starting with Windows XP, a "debug object" is created for a debugged process.
        /// Here's an example of checking for a "debug object" in the current process:
        /// If a debug object exists, then the process is being debugged.
        /// It was originally published on https://www.apriorit.com/
        /// </summary>
        public static unsafe bool CheckProcessDebugObjectHandle()
        {
            void* processInformation = null;
            uint returnLength = 0;
            var status = PInvoke.NtQueryInformationProcess
            (
                Process.GetCurrentProcess().SafeHandle,
                PROCESS_DEBUG_OBJECT_HANDLE,
                processInformation,
                (uint) sizeof(nint),
                ref returnLength
            );
            return status == 0 &&  processInformation != null;
        }
    }
}