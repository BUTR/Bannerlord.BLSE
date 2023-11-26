using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

internal static class ModuleInitializer
{
    private static int _isAttached;

    [ModuleInitializer]
    internal static void Action()
    {
        if (Interlocked.Exchange(ref _isAttached, 1) == 1)
            return;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs e)
    {
        if (e.Name is null)
            return null;

        var assemblyName = new AssemblyName(e.Name);
        if (assemblyName.Name is "System.Drawing.Common")
            return ResolveLauncherExAssemblies(assemblyName);
        if (assemblyName.Name is "System.Memory")
            return ResolveLauncherExAssemblies(assemblyName);
        if (assemblyName.Name is "System.Buffers")
            return ResolveLauncherExAssemblies(assemblyName);
        if (assemblyName.Name is "System.Runtime.CompilerServices.Unsafe")
            return ResolveLauncherExAssemblies(assemblyName);

        return null;
    }

    private static Assembly? ResolveLauncherExAssemblies(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;
        
        var @namespace = "Bannerlord.BLSE.Loaders.LauncherEx.";
        var resources = typeof(ModuleInitializer).Assembly.GetManifestResourceNames().Select(x => x.Remove(0, @namespace.Length));
        var toLoad = resources.FirstOrDefault(x => x.StartsWith(name));
        if (toLoad is not null)
        {
            using var resourceStream = typeof(ModuleInitializer).Assembly.GetManifestResourceStream($"{@namespace}{toLoad}")!;
            using var ms = new MemoryStream();
            resourceStream.CopyTo(ms);
            return Assembly.Load(ms.ToArray());
        }

        return null;
    }
}