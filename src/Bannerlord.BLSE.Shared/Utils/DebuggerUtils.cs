using Bannerlord.BLSE.Shared.Utils.DebuggerDetection;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bannerlord.BLSE.Shared.Utils
{
    public static class DebuggerUtils
    {
        public static bool IsDebuggerAttached()
        {
            if (Debugger.IsAttached)
                return true;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ProcessDebug.CheckProcessDebugObjectHandle();

            return false;
        }
    }
}