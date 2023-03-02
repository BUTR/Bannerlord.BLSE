using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace Bannerlord.BLSE.Shared;

internal static class ModuleInitializer
{
    private static int isAttached;

    [ModuleInitializer]
    internal static void Action()
    {
        if (Interlocked.Exchange(ref isAttached, 1) == 1)
        {
            return;
        }
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs e)
    {
        if (e.Name is null)
            return null;

        var assemblyName = new AssemblyName(e.Name);
#if NET472
        // On .NET Framework, keep the Mono.Cecil up to date.
        if (assemblyName.Name is not "0Harmony" and not "Mono.Cecil")
            return null;
#elif NETCOREAPP3_1_OR_GREATER
            // On .NET Core, harmony has split MonoMod.Common as a separate dependency.
            if (assemblyName.Name is not "0Harmony" and not "Mono.Cecil" and not "Mono.Cecil.Mdb" and not "Mono.Cecil.Mdb" and not "Mono.Cecil.Rocks" and not "MonoMod.Common")
                return null;
#endif

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName == assemblyName.FullName)
            {
                return assembly;
            }
        }

        var assemblyNameFull = $"{assemblyName.Name}.dll";

        var configName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
        var harmonyModuleFolder = Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "Modules", "Bannerlord.Harmony");
        if (!Directory.Exists(harmonyModuleFolder))
        {
            MessageBox.Show("The Harmony module is missing!\nCan't launch with 'Bannerlord.Harmony' module missing!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
            return null;
        }
        var harmonyFolder = Path.Combine(harmonyModuleFolder, "bin", configName);
        if (!Directory.Exists(harmonyFolder))
        {
            MessageBox.Show($"The Harmony module is corrupted!\nCan't find '{Path.Combine("bin", configName)}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
            return null;
        }

        var assemblyFile = Path.Combine(harmonyFolder, assemblyNameFull);
        if (!File.Exists(assemblyFile))
        {
            MessageBox.Show($"The Harmony module is corrupted!\nCan't find '{assemblyNameFull}' in 'Bannerlord.Harmony'!", "Error from BLSE!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
            return null;
        }

        return Assembly.LoadFrom(assemblyFile);
    }

    public static void Disable()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
    }
}