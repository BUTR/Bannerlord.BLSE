using Bannerlord.LauncherManager.External;

using System;
using System.IO;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class FileSystemProviderImpl : IFileSystemProvider
{
    public static readonly FileSystemProviderImpl Instance = new();

    public byte[]? ReadFileContent(string filePath, int offset, int length)
    {
        if (!File.Exists(filePath)) return null;

        try
        {
            if (offset == 0 && length == -1)
            {
                return File.ReadAllBytes(filePath);
            }
            else if (offset >= 0 && length > 0)
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var data = new byte[length];
                fs.Seek(offset, SeekOrigin.Begin);
                _ = fs.Read(data, 0, length);
                return data;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Bannerlord IO Read Operation failed! {Path}", filePath);
            return null;
        }
    }

    public void WriteFileContent(string filePath, byte[]? data)
    {
        try
        {
            if (data is null)
                File.Delete(filePath);
            else
                File.WriteAllBytes(filePath, data);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Bannerlord IO Write Operation failed! {Path}", filePath);
        }
    }

    public string[]? ReadDirectoryFileList(string directoryPath) => Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath) : null;

    public string[]? ReadDirectoryList(string directoryPath) => Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath) : null;
}