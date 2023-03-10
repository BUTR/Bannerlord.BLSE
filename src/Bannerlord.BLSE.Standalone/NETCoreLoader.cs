using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Bannerlord.BLSE;

public static class NETCoreLoader
{
    private const string CoreCLRPath = "Microsoft.NETCore.App/coreclr.dll";

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void EntryDelegate(int argc, IntPtr[] argv);

    [DllImport(CoreCLRPath, CallingConvention = CallingConvention.StdCall)]
    private static extern int coreclr_initialize(IntPtr exePath, IntPtr appDomainFriendlyName, int propertyCount, IntPtr[] propertyKeys, IntPtr[] propertyValues, out IntPtr hostHandle, out IntPtr domainId);

    [DllImport(CoreCLRPath, CallingConvention = CallingConvention.StdCall)]
    private static extern int coreclr_create_delegate(IntPtr hostHandle, uint domainId, IntPtr entryPointAssemblyName, IntPtr entryPointTypeName, IntPtr entryPointMethodName, out IntPtr @delegate);

    private static IntPtr NativeUTF8(string str)
    {
        var length = Encoding.UTF8.GetByteCount(str);
        var buffer = new byte[length + 1];
        Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
        buffer[buffer.Length - 1] = 0;

        var nativeBuffer = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeBuffer, buffer.Length);
        return nativeBuffer;
    }

    public static void Launch(string[] args)
    {
        // Disable aggressive inlining of JIT by disabling JIT Optimizations
        // TODO: This is kinda extreme. What could be done?
        Environment.SetEnvironmentVariable("COMPlus_JITMinOpts", "1");

        var path = AppDomain.CurrentDomain.BaseDirectory;
        var rootFiles = Directory.GetFiles(Path.Combine(path), "*.dll", SearchOption.TopDirectoryOnly)
            .Where(x => Path.GetFileName(x) != "Mono.Cecil.dll") // On .NET Core, the game distributes an old version of Mono.Cecil.dll. Ignore it.
            .Select(x => $"{x};");
        var netcoreFiles = Directory.GetFiles(Path.Combine(path, "Microsoft.NETCore.App"), "*.dll", SearchOption.TopDirectoryOnly).Select(x => $"{x};");
        var aspCoreFiles = Directory.GetFiles(Path.Combine(path, "Microsoft.AspNetCore.App"), "*.dll", SearchOption.TopDirectoryOnly).Select(x => $"{x};");
        var winDeskFiles = Directory.GetFiles(Path.Combine(path, "Microsoft.WindowsDesktop.App"), "*.dll", SearchOption.TopDirectoryOnly).Select(x => $"{x};");
        // Do not set Modules .dll as trusted and to be loaded
        var modulesFiles = Directory.GetFiles(Path.Combine(path, "..", "..", "Modules"), "*.dll", SearchOption.AllDirectories).Select(x => $"{x};");
        // The instantly loaded assembly files
        var files = string.Join("", rootFiles.Concat(netcoreFiles).Concat(aspCoreFiles).Concat(winDeskFiles)/*.Concat(files5)*/);

        var propKeys = new IntPtr[]
        {
            NativeUTF8("TRUSTED_PLATFORM_ASSEMBLIES")
        };
        var propValues = new IntPtr[]
        {
            NativeUTF8(files)
        };

        var initResult = coreclr_initialize(
            NativeUTF8(path),
            NativeUTF8("BLSE"),
            1,
            propKeys,
            propValues,
            out var pCLRRuntimeHost,
            out var domainId);
        if (initResult < 0)
        {
            Console.WriteLine("Failed to initialize Bannerlord's .NET Core CLR!");
            return;
        }

        var createDelegateResult = coreclr_create_delegate(
            pCLRRuntimeHost,
            (uint) domainId,
            NativeUTF8("Bannerlord.BLSE.Shared"),
            NativeUTF8("Bannerlord.BLSE.Shared.Program"),
            NativeUTF8("NativeEntry"),
            out var pMethod);
        if (createDelegateResult < 0)
        {
            Console.WriteLine("Failed to get BLSE's entrypoint!");
            return;
        }

        var args2 = args.Select(NativeUTF8).ToArray();
        var @delegate = Marshal.GetDelegateForFunctionPointer<EntryDelegate>(pMethod);
        @delegate(args.Length, args2);
    }
}