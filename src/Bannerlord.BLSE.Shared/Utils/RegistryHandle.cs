using Microsoft.Win32.SafeHandles;

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Bannerlord.BLSE.Shared.Utils;

file static class SafeNativeMethods
{
    internal const int ERROR_SUCCESS = 0;

    internal const int KEY_QUERY_VALUE = 1;
    internal const int KEY_SET_VALUE = 2;
    internal const int KEY_CREATE_SUB_KEY = 4;
    internal const int KEY_ENUMERATE_SUB_KEYS = 8;
    internal const int KEY_NOTIFY = 16;
    internal const int KEY_CREATE_LINK = 32;
    internal const int KEY_READ = 131097;
    internal const int KEY_WRITE = 131078;

    internal const int REG_NONE = 0;
    internal const int REG_SZ = 1;
    internal const int REG_EXPAND_SZ = 2;
    internal const int REG_BINARY = 3;
    internal const int REG_DWORD = 4;
    internal const int REG_DWORD_LITTLE_ENDIAN = 4;
    internal const int REG_DWORD_BIG_ENDIAN = 5;
    internal const int REG_LINK = 6;
    internal const int REG_MULTI_SZ = 7;
    internal const int REG_RESOURCE_LIST = 8;
    internal const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
    internal const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
    internal const int REG_QWORD = 11;

    internal const string ADVAPI32 = "advapi32.dll";

    [DllImport(ADVAPI32, BestFitMapping = false, CharSet = CharSet.Unicode)]
    internal static extern int RegOpenKeyEx(RegistryHandle hKey, string lpSubKey, int ulOptions, int samDesired, out RegistryHandle? hkResult);

    /*
	[DllImport(ADVAPI32, BestFitMapping = false, CharSet = CharSet.Unicode)]
	internal static extern int RegSetValueEx(RegistryHandle hKey, string lpValueName, int Reserved, int dwType, string val, int cbData);

	[DllImport(ADVAPI32, BestFitMapping = false, CharSet = CharSet.Unicode)]
	internal static extern int RegCreateKeyEx(RegistryHandle hKey, string lpSubKey, int Reserved, string? lpClass, int dwOptions, int samDesigner, IntPtr lpSecurityAttributes, out RegistryHandle hkResult, out int lpdwDisposition);
    */

    [DllImport(ADVAPI32)]
    internal static extern int RegCloseKey(IntPtr handle);

    [DllImport(ADVAPI32, BestFitMapping = false, CharSet = CharSet.Unicode)]
    internal static extern int RegQueryValueEx(RegistryHandle hKey, string lpValueName, int lpReserved, ref int lpType, [Out] byte[]? lpData, ref int lpcbData);

    /*
	[DllImport(ADVAPI32, BestFitMapping = false, CharSet = CharSet.Unicode)]
	internal static extern int RegDeleteKey(RegistryHandle hKey, string lpValueName);
    */
}

internal class RegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public static readonly RegistryHandle HKEY_CLASSES_ROOT = new(new IntPtr(int.MinValue), ownHandle: false);
    public static readonly RegistryHandle HKEY_CURRENT_USER = new(new IntPtr(-2147483647), ownHandle: false);
    public static readonly RegistryHandle HKEY_LOCAL_MACHINE = new(new IntPtr(-2147483646), ownHandle: false);
    public static readonly RegistryHandle HKEY_USERS = new(new IntPtr(-2147483645), ownHandle: false);
    public static readonly RegistryHandle HKEY_PERFORMANCE_DATA = new(new IntPtr(-2147483644), ownHandle: false);
    public static readonly RegistryHandle HKEY_CURRENT_CONFIG = new(new IntPtr(-2147483643), ownHandle: false);
    public static readonly RegistryHandle HKEY_DYN_DATA = new(new IntPtr(-2147483642), ownHandle: false);

    public static int TryGetHKLMSubkey(string key, out RegistryHandle? regHandle)
    {
        return SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, SafeNativeMethods.KEY_READ, out regHandle);
    }

    public static RegistryHandle? GetHKLMSubkey(string key)
    {
        if (SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, SafeNativeMethods.KEY_READ, out var hkResult) != SafeNativeMethods.ERROR_SUCCESS || hkResult == null || hkResult.IsInvalid)
            return null;
        return hkResult;
    }

    public static RegistryHandle? GetHKCUSubkey(string key)
    {
        if (SafeNativeMethods.RegOpenKeyEx(HKEY_CURRENT_USER, key, 0, SafeNativeMethods.KEY_READ, out var hkResult) != SafeNativeMethods.ERROR_SUCCESS || hkResult == null || hkResult.IsInvalid)
            return null;
        return hkResult;
    }

    public RegistryHandle(IntPtr hKey, bool ownHandle) : base(ownHandle) => handle = hKey;
    public RegistryHandle() : base(ownsHandle: true) { }

    /*
	public bool DeleteKey(string key)
	{
		if (SafeNativeMethods.RegDeleteKey(this, key) == SafeNativeMethods.ERROR_SUCCESS)
		{
			return true;
		}
		return false;
	}

	public RegistryHandle? CreateSubKey(string subKey)
	{
        if (SafeNativeMethods.RegCreateKeyEx(this, subKey, 0, null, 0, SafeNativeMethods.KEY_CREATE_SUB_KEY, IntPtr.Zero, out var hkResult, out _) != SafeNativeMethods.ERROR_SUCCESS || hkResult == null || hkResult.IsInvalid)
            return null;
		return hkResult;
	}

	public bool SetValue(string valName, string value)
	{
		if (SafeNativeMethods.RegSetValueEx(this, valName, 0, SafeNativeMethods.REG_SZ, value, value.Length * 2 + 2) != SafeNativeMethods.ERROR_SUCCESS)
            return false;
        return true;
    }
    */

    public string? GetStringValue(string valName)
    {
        var lpType = 0;
        var lpcbData = 0;
        if (SafeNativeMethods.RegQueryValueEx(this, valName, 0, ref lpType, null, ref lpcbData) == SafeNativeMethods.ERROR_SUCCESS && lpType == 1)
        {
            var array = new byte[lpcbData];
            var num = SafeNativeMethods.RegQueryValueEx(this, valName, 0, ref lpType, array, ref lpcbData);
            return Encoding.Unicode.GetString(array, 0, array.Length - 2);
        }
        return null;
    }

    public int? GetDwordValue(string valName)
    {
        var lpType = 4;
        var arraySize = 4;
        var array = new byte[arraySize];
        if (SafeNativeMethods.RegQueryValueEx(this, valName, 0, ref lpType, array, ref arraySize) == SafeNativeMethods.ERROR_SUCCESS && lpType == 4)
            return BitConverter.ToInt32(array, 0);
        return null;
    }

    protected override bool ReleaseHandle() => SafeNativeMethods.RegCloseKey(handle) == SafeNativeMethods.ERROR_SUCCESS;
}