using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Bannerlord.LauncherEx.Helpers;

internal readonly struct KeyboardState
{
    public static KeyboardState Empty = new(null);

    private static readonly byte[] _definedKeyCodes =
        ((Keys[]) Enum.GetValues(typeof(Keys))).Cast<int>().Where(keyCode => keyCode is >= 1 and <= 255).Select(keyCode => (byte) keyCode).ToArray();

    private const byte CapsLockModifier = 1;
    private const byte NumLockModifier = 2;

    private readonly IMemoryOwner<byte>? _keyState;
    // Array of 256 bits:
    private readonly uint _keys0, _keys1, _keys2, _keys3, _keys4, _keys5, _keys6, _keys7;
    private readonly byte _modifiers;

    public bool CapsLock => (_modifiers & CapsLockModifier) > 0;
    public bool NumLock => (_modifiers & NumLockModifier) > 0;
    public KeyState this[Keys key] => InternalGetKey(key) ? KeyState.Down : KeyState.Up;

    public KeyboardState(IMemoryOwner<byte>? keyState, bool capsLock = false, bool numLock = false) : this()
    {
        _keyState = keyState;
        _keys0 = 0;
        _keys1 = 0;
        _keys2 = 0;
        _keys3 = 0;
        _keys4 = 0;
        _keys5 = 0;
        _keys6 = 0;
        _keys7 = 0;
        _modifiers = (byte) (0 | (capsLock ? CapsLockModifier : 0) | (numLock ? NumLockModifier : 0));

        if (_keyState is null) return;
        var keys = new HashSet<Keys>();
        var span = _keyState.Memory.Span;
        for (var i = 0; i < _definedKeyCodes.Length; i++)
        {
            if ((span[_definedKeyCodes[i]] & 0x80) == 0) continue;
            var key = (Keys) _definedKeyCodes[i];
            if (keys.Contains(key)) continue;
            keys.Add(key);

            var mask = (uint) 1 << ((int) key & 0x1f);
            switch ((int) key >> 5)
            {
                case 0: _keys0 |= mask; break;
                case 1: _keys1 |= mask; break;
                case 2: _keys2 |= mask; break;
                case 3: _keys3 |= mask; break;
                case 4: _keys4 |= mask; break;
                case 5: _keys5 |= mask; break;
                case 6: _keys6 |= mask; break;
                case 7: _keys7 |= mask; break;
            }
        }
    }

    public bool IsKeyDown(Keys key) => InternalGetKey(key);
    public bool IsKeyUp(Keys key) => !InternalGetKey(key);

    public int GetPressedKeyCount()
    {
        var count = CountBits(_keys0) + CountBits(_keys1) + CountBits(_keys2) + CountBits(_keys3)
                    + CountBits(_keys4) + CountBits(_keys5) + CountBits(_keys6) + CountBits(_keys7);
        return (int) count;
    }
    public Keys[] GetPressedKeys()
    {
        var count = GetPressedKeyCount();
        if (count == 0) return Array.Empty<Keys>();
        var keys = new Keys[count];

        var index = 0;
        if (_keys0 != 0) index = AddKeysToArray(_keys0, 0 * 32, keys, index);
        if (_keys1 != 0) index = AddKeysToArray(_keys1, 1 * 32, keys, index);
        if (_keys2 != 0) index = AddKeysToArray(_keys2, 2 * 32, keys, index);
        if (_keys3 != 0) index = AddKeysToArray(_keys3, 3 * 32, keys, index);
        if (_keys4 != 0) index = AddKeysToArray(_keys4, 4 * 32, keys, index);
        if (_keys5 != 0) index = AddKeysToArray(_keys5, 5 * 32, keys, index);
        if (_keys6 != 0) index = AddKeysToArray(_keys6, 6 * 32, keys, index);
        if (_keys7 != 0) index = AddKeysToArray(_keys7, 7 * 32, keys, index);

        return keys;
    }

    private bool InternalGetKey(Keys key)
    {
        var mask = (uint) 1 << ((int) key & 0x1f);
        uint element = ((int) key >> 5) switch
        {
            0 => _keys0,
            1 => _keys1,
            2 => _keys2,
            3 => _keys3,
            4 => _keys4,
            5 => _keys5,
            6 => _keys6,
            7 => _keys7,
            _ => 0
        };
        return (element & mask) != 0;
    }

    public override int GetHashCode() => (int) (_keys0 ^ _keys1 ^ _keys2 ^ _keys3 ^ _keys4 ^ _keys5 ^ _keys6 ^ _keys7);

    public static bool operator ==(KeyboardState a, KeyboardState b) => a._keys0 == b._keys0 &&
                                                                        a._keys1 == b._keys1 &&
                                                                        a._keys2 == b._keys2 &&
                                                                        a._keys3 == b._keys3 &&
                                                                        a._keys4 == b._keys4 &&
                                                                        a._keys5 == b._keys5 &&
                                                                        a._keys6 == b._keys6 &&
                                                                        a._keys7 == b._keys7;

    public static bool operator !=(KeyboardState a, KeyboardState b) => !(a == b);

    public override bool Equals(object? obj) => obj is KeyboardState state && this == state;

    public unsafe string AsString(Keys key)
    {
        if (_keyState is null)
            return string.Empty;

        var vkCode = (uint) key;

        var currentHWnd = PInvoke.GetForegroundWindow();
        var currentWindowThreadID = PInvoke.GetWindowThreadProcessId(currentHWnd);

        var hkl = PInvoke.GetKeyboardLayout(currentWindowThreadID);
        var lScanCode = PInvoke.MapVirtualKeyEx(vkCode, 0, hkl);

        var span = stackalloc char[5];
        var relevantKeyCountInBuffer = PInvoke.ToUnicodeEx(vkCode, lScanCode, _keyState.Memory.Span, new PWSTR(span), 5, 0, hkl);
        return relevantKeyCountInBuffer switch
        {
            -1 or 0 => string.Empty,
            1 => span[0].ToString(),
            2 or _ => new string(span, 0, 2),
        };
    }

    private static uint CountBits(uint v)
    {
        // http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
        v = v - ((v >> 1) & 0x55555555);                       // reuse input as temporary
        v = (v & 0x33333333) + ((v >> 2) & 0x33333333);        // temp
        return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
    }

    private static int AddKeysToArray(uint keys, int offset, Keys[] pressedKeys, int index)
    {
        for (var i = 0; i < 32; i++)
        {
            if ((keys & (1 << i)) != 0)
                pressedKeys[index++] = (Keys) (offset + i);
        }
        return index;
    }

    public void Dispose()
    {
        _keyState?.Dispose();
    }
}