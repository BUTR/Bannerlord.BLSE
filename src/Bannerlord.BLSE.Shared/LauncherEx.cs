using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.BLSE.Features.Xbox;
using Bannerlord.BLSE.Shared.NoExceptions;
using Bannerlord.LauncherEx;

using HarmonyLib;

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Bannerlord.BLSE.Shared;

public static class LauncherEx
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

    public static void Launch(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();

        InterceptorFeature.Enable(_featureHarmony);
        AssemblyResolverFeature.Enable(_featureHarmony);
        ContinueSaveFileFeature.Enable(_featureHarmony);
        CommandsFeature.Enable(_featureHarmony);
        XboxFeature.Enable(_featureHarmony);

        Manager.Initialize();
        Manager.Enable();

        ModuleInitializer.Disable();
        if (args.Contains("/noexceptions"))
        {
            ProgramEx.Main(args);
        }
        else
        {
            TaleWorlds.MountAndBlade.Launcher.Library.Program.Main(args);
        }
    }

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