using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Bannerlord.BLSE;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRRuntimeHost
{
#pragma warning disable CS0649
    private readonly ICLRRuntimeHostVtbl* vtbl;
#pragma warning restore CS0649
    
    public static nint Release(ICLRRuntimeHost* host)
    {
        var release = (delegate*<ICLRRuntimeHost*, nint>) host->vtbl->Release;
        return release(host);
    }
    
    public static nint Start(ICLRRuntimeHost* host)
    {
        var start = (delegate*<ICLRRuntimeHost*, nint>) host->vtbl->Start;
        return start(host);
    }
    
    public static int ExecuteInDefaultAppDomain(ICLRRuntimeHost* host, string dllPath, string typeName, string methodName, string argument)
    {
        fixed (char* pDllPath = dllPath)
        fixed (char* pTypeName = typeName)
        fixed (char* pMethodName = methodName)
        fixed (char* pArgument = argument)
        {
            var executeInDefaultAppDomain = (delegate*<ICLRRuntimeHost*, char*, char*, char*, char*, int*, nint>) host->vtbl->ExecuteInDefaultAppDomain;
            
            int returnVal;
            var result = executeInDefaultAppDomain(host, pDllPath, pTypeName, pMethodName, pArgument, &returnVal);
            return result == 0 ? returnVal : 1;
        }
    }
}
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRRuntimeHostVtbl
{
#pragma warning disable CS0649
    public readonly void* QueryInterface;
    public readonly void* AddRef;
    public readonly void* Release;
    public readonly void* Start;
    public readonly void* Stop;
    public readonly void* SetHostControl;
    public readonly void* GetCLRControl;
    public readonly void* UnloadAppDomain;
    public readonly void* ExecuteInAppDomain;
    public readonly void* GetCurrentAppDomainId;
    public readonly void* ExecuteApplication;
    public readonly void* ExecuteInDefaultAppDomain;
#pragma warning restore CS0649
}

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRRuntimeInfo
{
    private static readonly Guid CLSID_CLRRuntimeHost = new("90F1A06E-7712-4762-86B5-7A5EBA6BDB02");
    private static readonly Guid IID_ICLRRuntimeHost = new("90F1A06C-7712-4762-86B5-7A5EBA6BDB02");
    
#pragma warning disable CS0649
    private readonly ICLRRuntimeInfoVtbl* vtbl;
#pragma warning restore CS0649

    public static nint Release(ICLRRuntimeInfo* host)
    {
        var release = (delegate*<ICLRRuntimeInfo*, nint>) host->vtbl->Release;
        return release(host);
    }
    
    private static T* GetInterface<T>(ICLRRuntimeInfo* host, Guid* rclsid, Guid* riid) where T : unmanaged
    {
        var getInterface = (delegate*<ICLRRuntimeInfo*, Guid*, Guid*, T**, nint>) host->vtbl->GetInterface;
        
        T* ptr;
        var result = getInterface(host, rclsid, riid, &ptr);
        return ptr;
    }
    
    public static ICLRRuntimeHost* GetRuntimeHost(ICLRRuntimeInfo* host)
    {
        fixed (Guid* clsid = &CLSID_CLRRuntimeHost)
        fixed (Guid* iid = &IID_ICLRRuntimeHost)
        {
            return GetInterface<ICLRRuntimeHost>(host, clsid, iid);
        }
    }
}
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRRuntimeInfoVtbl
{
#pragma warning disable CS0649
    public readonly void* QueryInterface;
    public readonly void* AddRef;
    public readonly void* Release;
    public readonly void* GetVersionString;
    public readonly void* GetRuntimeDirectory;
    public readonly void* IsLoaded;
    public readonly void* LoadErrorString;
    public readonly void* LoadLibrary;
    public readonly void* GetProcAddress;
    public readonly void* GetInterface;
    public readonly void* IsLoadable;
    public readonly void* SetDefaultStartupFlags;
    public readonly void* GetDefaultStartupFlags;
    public readonly void* BindAsLegacyV2Runtime;
    public readonly void* IsStarted;
#pragma warning restore CS0649
}

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRMetaHost
{
    private static readonly Guid CLSID_CLRMetaHost = new("9280188D-0E8E-4867-B30C-7FA83884E8DE");
    private static readonly Guid IID_ICLRMetaHost = new("D332DB9E-B9B3-4125-8207-A14884F53216");
    
    private static readonly Guid IID_ICLRRuntimeInfo = new("BD39D1D2-BA2F-486A-89B0-B4B0CB466891");

    [DllImport("mscoree")]
    private static extern int CLRCreateInstance(Guid* clsId, Guid* rIId, ICLRMetaHost** instance);
    
#pragma warning disable CS0649
    private readonly ICLRMetaHostVtbl *vtbl;
#pragma warning restore CS0649

    public static ICLRMetaHost* Create()
    {
        fixed (Guid* clsid = &CLSID_CLRMetaHost)
        fixed (Guid* iid = &IID_ICLRMetaHost)
        {
            ICLRMetaHost* host;
            var result = CLRCreateInstance(clsid, iid, &host);
            return host;
        }
    }
    
    public static nint Release(ICLRMetaHost* host)
    {
        var release = (delegate*<ICLRMetaHost*, nint>) host->vtbl->Release;
        return release(host);
    }
    
    public static ICLRRuntimeInfo* GetRuntime(ICLRMetaHost* host, string str)
    {
        fixed (char* pStr = str)
        fixed (Guid* iid = &IID_ICLRRuntimeInfo)
        {
            var getRuntime = (delegate*<ICLRMetaHost*, char*, Guid*, ICLRRuntimeInfo**, nint>) host->vtbl->GetRuntime;
            
            ICLRRuntimeInfo* ptr;
            var result = getRuntime(host, pStr, iid, &ptr);
            return result == 0 ? ptr : null;
        }
    }
}
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
[SuppressMessage("ReSharper", "UnusedVariable")]
file readonly unsafe struct ICLRMetaHostVtbl
{
#pragma warning disable CS0649
    public readonly void* QueryInterface;
    public readonly void* AddRef;
    public readonly void* Release;
    public readonly void* GetRuntime;
    public readonly void* GetVersionFromFile;
    public readonly void* EnumerateInstalledRuntimes;
    public readonly void* EnumerateLoadedRuntimes;
    public readonly void* RequestRuntimeLoadedNotification;
    public readonly void* QueryLegacyV2RuntimeBinding;
    public readonly void* ExitProcess;
#pragma warning restore CS0649
}

[SuppressMessage("ReSharper", "UnusedVariable")]
public static unsafe class NETFrameworkLoader
{
    public static void Launch(string[] args)
    {
        // Catch AccessViolation
        Environment.SetEnvironmentVariable("COMPlus_legacyCorruptedStateExceptionsPolicy", "1");

        var clrMetaHost = ICLRMetaHost.Create();
        var runtimeInfo = ICLRMetaHost.GetRuntime(clrMetaHost, "v4.0.30319");
        var runtimeHost = ICLRRuntimeInfo.GetRuntimeHost(runtimeInfo);
        var startResult = ICLRRuntimeHost.Start(runtimeHost);
        var executeResult = ICLRRuntimeHost.ExecuteInDefaultAppDomain(runtimeHost, "Bannerlord.BLSE.Shared.dll", "Bannerlord.BLSE.Shared.Program", "NativeEntry2", string.Join("|||", args));
        ICLRRuntimeInfo.Release(runtimeInfo);
        ICLRRuntimeHost.Release(runtimeHost);
        ICLRMetaHost.Release(clrMetaHost);
    }
}