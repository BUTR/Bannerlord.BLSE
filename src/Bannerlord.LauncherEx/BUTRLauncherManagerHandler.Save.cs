using Bannerlord.LauncherEx.Extensions;
using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Bannerlord.LauncherEx;

partial class BUTRLauncherManagerHandler
{
    public override Task<IReadOnlyList<SaveMetadata>> GetSaveFilesAsync() => Task.FromResult<IReadOnlyList<SaveMetadata>>(MBSaveLoad.GetSaveFiles().Where(x => x.MetaData is not null).Select(x =>
    {
        var dict = new SaveMetadata(x.Name);
        foreach (var key in x.MetaData.Keys)
            dict.Add(key, x.MetaData[key]);
        return dict;
    }).ToArray());

    public override Task<SaveMetadata?> GetSaveMetadataAsync(string fileName, ReadOnlyMemory<byte> data)
    {
        using var stream = new MemoryStream(data.ToArray());
        var metadata = MetaData.Deserialize(stream);
        var dict = new SaveMetadata(fileName);
        foreach (var key in metadata.Keys)
            dict.Add(key, metadata[key]);
        return Task.FromResult<SaveMetadata?>(dict);
    }

    private static string? GetSaveFilePath(SaveGameFileInfo saveGameFileInfo)
    {
        var savesDirectory = new PlatformDirectoryPath(PlatformFileType.User, "Game Saves\\");
        if (PlatformFileHelperPCExtended.GetDirectoryFullPath(savesDirectory) is not { } savesDirectoryPath) return null;
        return Path.Combine(savesDirectoryPath, $"{saveGameFileInfo.Name}.sav");
    }

    public override Task<string?> GetSaveFilePathAsync(string saveFile)
    {
        if (MBSaveLoad.GetSaveFileWithName(saveFile) is not { } si || GetSaveFilePath(si) is not { } saveFilePath || !File.Exists(saveFilePath))
            return Task.FromResult<string?>(null);
        return Task.FromResult<string?>(saveFilePath);
    }
}