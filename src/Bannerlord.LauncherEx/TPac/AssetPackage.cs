using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bannerlord.LauncherEx.TPac;

internal class AssetPackage
{
    public const uint TPAC_MAGIC_NUMBER = 0x43415054;

    public bool HeaderLoaded { get; private set; } = false;

    public FileInfo File { get; }

    public List<AssetItem> Items { get; } = new();

    public AssetPackage(string filePath)
    {
        File = new FileInfo(filePath);
    }

    public Texture? GetTexture(string id)
    {
        Load();
        return Items.OfType<Texture>().FirstOrDefault(x => x.Name == id);
    }

    public void Load()
    {
        if (!HeaderLoaded)
        {
            using var stream = File.OpenBinaryReader();
            Load(stream);
        }
    }

    protected virtual void Load(BinaryReader stream)
    {
        if (!stream.BaseStream.CanSeek)
            throw new IOException("The base stream must support random access (seek).");
        HeaderLoaded = true;

        if (stream.ReadUInt32() != TPAC_MAGIC_NUMBER)
            throw new IOException($"Not a Tpac file: {File.FullName}");
        var version = stream.ReadUInt32();
        switch (version)
        {
            case 1: // 1.0.0~1.4.2
            case 2: // since 1.4.3
                break;
            default:
                throw new Exception("Unsupported Tpac version: " + version);
        }

        stream.ReadGuid();

        var resourceNum = stream.ReadUInt32();
        var dataOffset = stream.ReadUInt32();
        stream.ReadUInt32(); // skip reserve

        for (var i = 0; i < resourceNum; i++)
        {
            var typeGuid = stream.ReadGuid();
            TypedAssetFactory.CreateTypedAsset(typeGuid, out var assetItem);
            assetItem.Guid = stream.ReadGuid();

            uint assetVersion = 0;
            if (version > 1) assetVersion = stream.ReadUInt32();
            assetItem.Version = assetVersion;
            assetItem.Name = stream.ReadSizedString();

            var metadataSize = stream.ReadUInt64();
            assetItem.ReadMetadata(stream, (int) metadataSize);
            var unknownMetadataChecknum = stream.ReadInt64();

            var dataSegmentNum = stream.ReadInt32();
            var segments = new AbstractExternalLoader[dataSegmentNum];
            for (var j = 0; j < dataSegmentNum; j++)
            {
                var segOffset = stream.ReadUInt64();
                var segActualSize = stream.ReadUInt64();
                var segStorageSize = stream.ReadUInt64();
                var segGuid = stream.ReadGuid();
                var segTypeGuid = stream.ReadGuid();
                TypedDataFactory.CreateTypedLoader(segTypeGuid, File, out var result);
                result._offset = segOffset;
                result._actualSize = segActualSize;
                result._storageSize = segStorageSize;
                result.OwnerGuid = segGuid;
                stream.ReadUInt64();
                stream.ReadUInt32();
                result._storageFormat = (AbstractExternalLoader.StorageFormat) stream.ReadByte();
                segments[j] = result;
            }

            assetItem.ConsumeDataSegments(segments);

            var depNum = stream.ReadInt32();
            stream.BaseStream.Seek(depNum * 3 * Unsafe.SizeOf<Guid>(), SeekOrigin.Current);
            Items.Add(assetItem);
        }
    }
}