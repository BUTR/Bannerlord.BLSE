using System.Runtime.InteropServices;

namespace Bannerlord.BLSE;

file unsafe struct ICLRRuntimeHost
{
    private ICLRRuntimeHostVtbl* vtbl;
    
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
file unsafe struct ICLRRuntimeHostVtbl
{
    public void* QueryInterface;
    public void* AddRef;
    public void* Release;
    public void* Start;
    public void* Stop;
    public void* SetHostControl;
    public void* GetCLRControl;
    public void* UnloadAppDomain;
    public void* ExecuteInAppDomain;
    public void* GetCurrentAppDomainId;
    public void* ExecuteApplication;
    public void* ExecuteInDefaultAppDomain;
}

file unsafe struct ICLRRuntimeInfo
{
    private static Guid CLSID_CLRRuntimeHost = new("90F1A06E-7712-4762-86B5-7A5EBA6BDB02");
    private static Guid IID_ICLRRuntimeHost = new("90F1A06C-7712-4762-86B5-7A5EBA6BDB02");
    
    private ICLRRuntimeInfoVtbl* vtbl;
    
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
file unsafe struct ICLRRuntimeInfoVtbl
{
    public void* QueryInterface;
    public void* AddRef;
    public void* Release;
    public void* GetVersionString;
    public void* GetRuntimeDirectory;
    public void* IsLoaded;
    public void* LoadErrorString;
    public void* LoadLibrary;
    public void* GetProcAddress;
    public void* GetInterface;
    public void* IsLoadable;
    public void* SetDefaultStartupFlags;
    public void* GetDefaultStartupFlags;
    public void* BindAsLegacyV2Runtime;
    public void* IsStarted;
}

file unsafe struct ICLRMetaHost
{
    private static Guid CLSID_CLRMetaHost = new("9280188D-0E8E-4867-B30C-7FA83884E8DE");
    private static Guid IID_ICLRMetaHost = new("D332DB9E-B9B3-4125-8207-A14884F53216");
    
    private static Guid IID_ICLRRuntimeInfo = new("BD39D1D2-BA2F-486A-89B0-B4B0CB466891");

    [DllImport("mscoree")]
    private static extern int CLRCreateInstance(Guid* clsId, Guid* rIId, ICLRMetaHost** instance);
    
    private ICLRMetaHostVtbl *vtbl;

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
file unsafe struct ICLRMetaHostVtbl
{
    public void* QueryInterface;
    public void* AddRef;
    public void* Release;
    public void* GetRuntime;
    public void* GetVersionFromFile;
    public void* EnumerateInstalledRuntimes;
    public void* EnumerateLoadedRuntimes;
    public void* RequestRuntimeLoadedNotification;
    public void* QueryLegacyV2RuntimeBinding;
    public void* ExitProcess;
}

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