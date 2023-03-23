using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Bannerlord.LauncherEx.Helpers;

internal static class WindowsClipboard
{
    private const uint CFUnicodeText = 13;

    public static void SetText(string text)
    {
        TryOpenClipboard();

        InnerSet(text);
    }

    private static unsafe void InnerSet(string text)
    {
        PInvoke.EmptyClipboard();
        var hGlobal = HANDLE.Null;
        try
        {
            var bytes = (text.Length + 1) * 2;
            hGlobal = (HANDLE) Marshal.AllocHGlobal(bytes);

            if (hGlobal == IntPtr.Zero)
            {
                ThrowWin32();
            }

            var target = new IntPtr(PInvoke.GlobalLock(hGlobal));

            if (target == IntPtr.Zero)
            {
                ThrowWin32();
            }

            try
            {
                Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
            }
            finally
            {
                PInvoke.GlobalUnlock(target);
            }

            if (PInvoke.SetClipboardData(CFUnicodeText, hGlobal) == IntPtr.Zero)
            {
                ThrowWin32();
            }

            hGlobal = HANDLE.Null;
        }
        finally
        {
            if (hGlobal != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(hGlobal);
            }

            PInvoke.CloseClipboard();
        }
    }

    private static void TryOpenClipboard()
    {
        var num = 10;
        while (true)
        {
            if (PInvoke.OpenClipboard(HWND.Null))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            Thread.Sleep(100);
        }
    }

    public static string? GetText()
    {
        if (!PInvoke.IsClipboardFormatAvailable(CFUnicodeText))
        {
            return null;
        }
        TryOpenClipboard();

        return InnerGet();
    }

    private static unsafe string? InnerGet()
    {
        var handle = IntPtr.Zero;

        var pointer = IntPtr.Zero;
        try
        {
            handle = PInvoke.GetClipboardData(CFUnicodeText);
            if (handle == IntPtr.Zero)
            {
                return null;
            }

            pointer = new IntPtr(PInvoke.GlobalLock(handle));
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            var size = (int) PInvoke.GlobalSize(handle);
            var buff = new byte[size];

            Marshal.Copy(pointer, buff, 0, size);

            return Encoding.Unicode.GetString(buff).TrimEnd('\0');
        }
        finally
        {
            if (pointer != IntPtr.Zero)
            {
                PInvoke.GlobalUnlock(handle);
            }

            PInvoke.CloseClipboard();
        }
    }

    private static void ThrowWin32() => throw new Win32Exception(Marshal.GetLastWin32Error());
}