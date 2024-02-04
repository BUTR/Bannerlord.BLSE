using Bannerlord.BLSE.Features.ExceptionInterceptor;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Bannerlord.BLSE.Shared.Utils;

internal static class LauncherExceptionHandler
{
    private static readonly Harmony _harmony = new("Bannerlord.BLSE.Shared.Patches.LauncherExceptionHandler");

    public static void Watch()
    {
        var asm = Assembly.LoadFrom("TaleWorlds.Starter.Library.dll");
        Trace.Assert(asm is not null);

        _harmony.TryPatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            prefix: AccessTools2.DeclaredMethod(typeof(LauncherExceptionHandler), nameof(MainPrefix)));

        // If ButterLib/BEW is not available or the stage is too early, use our built-in just in case
        ExceptionInterceptorFeature.Enable();
        ExceptionInterceptorFeature.OnException += ExceptionInterceptorFeatureOnException;
    }

    private static void ExceptionInterceptorFeatureOnException(Exception exception)
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
Version: {typeof(LauncherExceptionHandler).Assembly.GetName().Version}
{GetRecursiveException(exception)}");
    }

    private static void MainPrefix()
    {
        // After launch we rely on ButterLib/BEW being available
        ExceptionInterceptorFeature.OnException -= ExceptionInterceptorFeatureOnException;
        ExceptionInterceptorFeature.Disable();

        _harmony.Unpatch(
            AccessTools2.DeclaredMethod("TaleWorlds.Starter.Library.Program:Main"),
            AccessTools2.DeclaredMethod(typeof(LauncherExceptionHandler), nameof(MainPrefix)));
    }
}