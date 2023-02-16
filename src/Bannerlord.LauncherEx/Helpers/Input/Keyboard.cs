using System;
using System.Buffers;

using Windows.Win32;

namespace Bannerlord.LauncherEx.Helpers
{
    internal static class Keyboard
    {
        public static KeyboardState GetState()
        {
            var keyState = MemoryPool<byte>.Shared.Rent(256);
            return !PInvoke.GetKeyboardState(keyState.Memory.Span)
                ? KeyboardState.Empty
                : new KeyboardState(keyState, Console.CapsLock, Console.NumberLock);
        }
    }
}