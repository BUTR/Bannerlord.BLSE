using System;
using System.IO;

using Windows.Win32;
using Windows.Win32.System.Memory;

namespace Bannerlord.BLSE.Shared.Utils
{
    internal static unsafe class WatchdogHandler
    {
        private static readonly string WatchdogLibraryName = "TaleWorlds.Native.dll";
        private static readonly byte[] WatchdogOriginal = @"Watchdog\Watchdog.exe"u8.ToArray();
        private static readonly byte[] WatchdogReplacement = @"Wetchdog\Watchdog.exe"u8.ToArray();

        // Disable Watchdog by renaming it, thus performing a soft delete in it's eyes
        public static void DisableTWWatchdog()
        {
            var watchdogLibraryFile = new FileInfo(WatchdogLibraryName);
            if (!watchdogLibraryFile.Exists) return;

            var libraryHandle = PInvoke.LoadLibrary(WatchdogLibraryName);
            var success = false;
            libraryHandle.DangerousAddRef(ref success); // Never release it
            var libraryPtr = (byte*) libraryHandle.DangerousGetHandle().ToPointer();

            // Don't like the fact that I can't get the concrete in-memory size
            var size = (int) watchdogLibraryFile.Length;

            var librarySpan = new ReadOnlySpan<byte>(libraryPtr, size);

            var searchSpan = librarySpan;
            var searchSpanOffset = 0;
            while (searchSpan.IndexOf(WatchdogOriginal) is var idx and not -1)
            {
                var watchdogLocationPtr = libraryPtr + searchSpanOffset + idx;
                var watchdogLocationSpan = new Span<byte>(watchdogLocationPtr, WatchdogOriginal.Length);

                PInvoke.VirtualProtect(watchdogLocationPtr, (nuint) watchdogLocationSpan.Length, PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE, out var old);
                WatchdogReplacement.CopyTo(watchdogLocationSpan);
                PInvoke.VirtualProtect(watchdogLocationPtr, (nuint) watchdogLocationSpan.Length, old, out _);

                searchSpanOffset += idx + 1;
                searchSpan = searchSpan.Slice(idx + 1);
            }
        }
    }
}