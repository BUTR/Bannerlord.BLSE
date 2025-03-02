using Bannerlord.LauncherManager.External;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class FileSystemProviderImpl : IFileSystemProvider
{
    public static readonly FileSystemProviderImpl Instance = new();

    public Task<byte[]?> ReadFileContentAsync(string filePath, int position, int length)
    {
        FileStream? fileStream = null;
        try
        {
            fileStream = File.OpenRead(filePath);

            if (length == -1)
                length = (int) fileStream.Length;

            if (length == 0)
                return Task.FromResult<byte[]?>([]);

            var buffer = new byte[length];
            fileStream.Seek(position, SeekOrigin.Begin);
            var offset = 0;
            while (length > 0)
            {
                var read = fileStream.Read(buffer, offset, length);
                if (read == 0) break;
                offset += read;
                length -= read;
            }
            return Task.FromResult<byte[]?>(buffer);
        }
        catch
        {
            return Task.FromResult<byte[]?>(null);
        }
        finally
        {
            fileStream?.Dispose();
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
        catch (Exception)
        {
            //_logger.LogError(ex, "Bannerlord IO Write Operation failed! {Path}", filePath);
        }
        return Task.CompletedTask;
    }

    public Task<string[]?> ReadDirectoryFileListAsync(string directoryPath) => Task.FromResult(Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath) : null);

    public Task<string[]?> ReadDirectoryListAsync(string directoryPath) => Task.FromResult(Directory.Exists(directoryPath) ? Directory.GetDirectories(directoryPath) : null);
}