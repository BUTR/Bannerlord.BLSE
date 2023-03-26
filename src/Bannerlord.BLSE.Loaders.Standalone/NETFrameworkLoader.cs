#if !NETFRAMEWORKHOSTING
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Bannerlord.BLSE;

[SuppressMessage("ReSharper", "UnusedVariable")]
public static class NETFrameworkLoader
{
    private delegate void Main(string[] args);

    public static void Launch(string[] args)
    {
        // Catch AccessViolation
        Environment.SetEnvironmentVariable("COMPlus_legacyCorruptedStateExceptionsPolicy", "1");

        NtfsUnblocker.UnblockFile("Bannerlord.BLSE.Shared.dll");
        var sharedAssembly = Assembly.LoadFrom("Bannerlord.BLSE.Shared.dll");
        var sharedMainDelegate = (Main) Delegate.CreateDelegate(typeof(Main), sharedAssembly.GetType("Bannerlord.BLSE.Shared.Program"), "Main");
        sharedMainDelegate(args);
    }
}
#endif