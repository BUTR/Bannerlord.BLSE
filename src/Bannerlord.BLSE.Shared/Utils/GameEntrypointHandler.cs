using Bannerlord.BLSE.Features.ExceptionInterceptor;

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;

namespace Bannerlord.BLSE.Shared.Utils;

file enum CustomExceptionType : uint
{
    None = 0,
    StackOverflow = 1,
    AccessViolation = 2,
    Unknown = 3,
}

public static class GameEntrypointHandler
{
    private const int EXCEPTION_CONTINUE_SEARCH = 0;
    private const int EXCEPTION_EXECUTE_HANDLER = 1;
    private const int EXCEPTION_CONTINUE_EXECUTION = -1;

    private const uint STATUS_STACK_OVERFLOW = 0xC00000FD;
    private const uint STATUS_ACCESS_VIOLATION = 0xC0000005;

    private const uint STATUS_CUSTOM_STACK_OVERFLOW = 0xF00000FD;
    private const uint STATUS_CUSTOM_ACCESS_VIOLATION = 0xF00000FF;

    private static readonly PVECTORED_EXCEPTION_HANDLER HandlerDelegate;

    static unsafe GameEntrypointHandler()
    {
        HandlerDelegate = Handler;
        GC.KeepAlive(HandlerDelegate);
    }

    private static unsafe int Handler(EXCEPTION_POINTERS* ExceptionInfo)
    {
        if (ExceptionInfo == null)
            return EXCEPTION_CONTINUE_SEARCH;

        if (ExceptionInfo->ExceptionRecord == null)
            return EXCEPTION_CONTINUE_SEARCH;

        var record = ExceptionInfo->ExceptionRecord;
        Console.WriteLine(record->ExceptionCode);

        if (record->ExceptionCode == STATUS_STACK_OVERFLOW)
        {
            record->ExceptionCode = (NTSTATUS) STATUS_CUSTOM_STACK_OVERFLOW;
            return EXCEPTION_EXECUTE_HANDLER;
        }

        if (record->ExceptionCode == STATUS_ACCESS_VIOLATION)
        {
            record->ExceptionCode = (NTSTATUS) STATUS_CUSTOM_ACCESS_VIOLATION;
            return EXCEPTION_EXECUTE_HANDLER;
        }

        return EXCEPTION_CONTINUE_SEARCH;
    }


    public static unsafe int Entrypoint(string[] args)
    {
        // This doesn't work as intended, so let's not overcomplicate for now
        return GameOriginalEntrypointHandler.Entrypoint(args);


        // Setting the vectored exception handler to 'first' will cause
        // an ExecutionEngineException if the dotnet debugger is attached.
        // Note that hooking will not work properly if the handler is not
        // first because any of other application exception handlers may
        // change state in unpredictable ways.
        if (DebuggerUtils.IsDebuggerAttached())
            return GameOriginalEntrypointHandler.Entrypoint(args);

        var handler = PInvoke.AddVectoredExceptionHandler(1, HandlerDelegate);
        if (handler == null)
        {
            ExceptionInterceptorFeature.HandleException(new Win32Exception("AddVectoredExceptionHandler failed"));
            return 1;
        }

        var size = 32768U;
        if (!PInvoke.SetThreadStackGuarantee(&size))
        {
            ExceptionInterceptorFeature.HandleException(new InsufficientExecutionStackException("SetThreadStackGuarantee failed", new Win32Exception()));
            return 1;
        }

        var val = default(int);
        var exType = CustomExceptionType.None;
        var exCatched = default(Exception);
        try
        {
            val = GameOriginalEntrypointHandler.Entrypoint(args);
        }
        catch (Exception ex)
        {
            exCatched = ex;
            exType = CustomExceptionType.Unknown;

            if (ex is SEHException seh)
            {
                exType = (uint) seh.HResult switch
                {
                    STATUS_CUSTOM_STACK_OVERFLOW => CustomExceptionType.StackOverflow,
                    STATUS_CUSTOM_ACCESS_VIOLATION => CustomExceptionType.AccessViolation,
                    _ => CustomExceptionType.Unknown,
                };
            }
        }

        PInvoke.RemoveVectoredExceptionHandler(handler);

        switch (exType)
        {
            case CustomExceptionType.None:
                return val;
            case CustomExceptionType.StackOverflow:
                ExceptionInterceptorFeature.HandleException(new StackOverflowException("Stack Overflow", exCatched));
                throw exCatched!;
            case CustomExceptionType.AccessViolation:
                ExceptionInterceptorFeature.HandleException(new AccessViolationException("Access Violation", exCatched));
                throw exCatched!;
            case CustomExceptionType.Unknown:
                ExceptionInterceptorFeature.HandleException(new Exception("Unhandled", exCatched));
                throw exCatched!;
            default:
                return 1;
        }
    }
}