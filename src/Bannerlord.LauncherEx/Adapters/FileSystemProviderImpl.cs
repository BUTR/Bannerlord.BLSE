using Bannerlord.LauncherManager.External;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class FileSystemProviderImpl : IFileSystemProvider
{
    public static readonly FileSystemProviderImpl Instance = new();

    public Task<byte[]?> ReadFileContentAsync(string filePath, int offset, int length)
    {
        if (!File.Exists(filePath)) return Task.FromResult<byte[]?>(null);

        try
        {
            if (offset == 0 && length == -1)
            {
                return Task.FromResult<byte[]?>(File.ReadAllBytes(filePath));
            }
            else if (offset >= 0 && length > 0)
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var data = new byte[length];
                fs.Seek(offset, SeekOrigin.Begin);
                _ = fs.Read(data, 0, length);
                return Task.FromResult<byte[]?>(data);
            }
            else
            {
                return Task.FromResult<byte[]?>(null);
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Bannerlord.LauncherEx: Read failed for '{filePath}': {e}");
            return Task.FromResult<byte[]?>(null);
        }
    }

    public Task WriteFileContentAsync(string filePath, byte[]? data)
    {
        try
        {
            if (data is null)
                File.Delete(filePath);
            else
                File.WriteAllBytes(filePath, data);
        }
        catch (Exception e)
        {
            // Logged but not rethrown: callers like IssuesChecker don't wrap WriteFileContentAsync
            // and would crash on propagation. Export paths verify the write by reading back, so
            // user-visible failure surfaces there even with the swallow.
            Trace.WriteLine($"Bannerlord.LauncherEx: Write failed for '{filePath}': {e}");
        }
        return Task.CompletedTask;
    }

    public Task<string[]?> ReadDirectoryFileListAsync(string directoryPath) =>
        Task.FromResult(Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath) : null);

    public Task<string[]?> ReadDirectoryListAsync(string directoryPath) =>
        Task.FromResult(Directory.Exists(directoryPath) ? Directory.GetDirectories(directoryPath) : null);
}
