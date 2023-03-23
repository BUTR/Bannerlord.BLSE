using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class LauncherExceptionHandler
{
    private static readonly Harmony _harmony = new("Bannerlord.BLSE.Shared.Patches.LauncherExceptionHandler");


    public static void Watch()
    {
        Assembly.Load(new AssemblyName("TaleWorlds.Starter.Library"));

        _harmony.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            prefix: AccessTools2.Method(typeof(Unblocker), nameof(MainPrefix)));

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    private static void MainPrefix()
    {
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;

        _harmony.Unpatch(AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"), AccessTools2.Method(typeof(Unblocker), nameof(MainPrefix)));
    }

    [HandleProcessCorruptedStateExceptions, SecurityCritical]
    private static void CurrentDomainOnUnhandledException(object? _, UnhandledExceptionEventArgs e)
    {
        static string GetRecursiveException(Exception ex) => new StringBuilder()
            .AppendLine()
            .AppendLine($"Type: {ex.GetType().FullName}")
            .AppendLine(!string.IsNullOrWhiteSpace(ex.Message) ? $"Message: {ex.Message}" : string.Empty)
            .AppendLine(!string.IsNullOrWhiteSpace(ex.Source) ? $"Source: {ex.Source}" : string.Empty)
            .AppendLine(!string.IsNullOrWhiteSpace(ex.StackTrace) ? $@"CallStack:{Environment.NewLine}{string.Join(Environment.NewLine, ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))}" : string.Empty)
            .AppendLine(ex.InnerException != null ? $@"{Environment.NewLine}{Environment.NewLine}Inner {GetRecursiveException(ex.InnerException)}" : string.Empty)
            .ToString();

        using var fs = File.Open("BLSE_lasterror.log", FileMode.OpenOrCreate, FileAccess.Write);
        fs.SetLength(0);
        using var writer = new StreamWriter(fs);
        writer.Write($@"BLSE Exception:
Version: {typeof(Program).Assembly.GetName().Version}
{(e.ExceptionObject is Exception ex ? GetRecursiveException(ex) : e.ToString())}");
    }
}