using System;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal class AssetItem
{
    public Guid Type { get; private set; }

    public uint Version { get; protected internal set; }

    public Guid Guid { get; set; }

    public string Name { get; set; }

    public AssetItem(Guid type)
    {
            Type = type;
            Guid = Guid.Empty;
            Name = string.Empty;
        }

    public virtual void ReadMetadata(BinaryReader stream, int totalSize)
    {
            stream.BaseStream.Seek(totalSize, SeekOrigin.Current);
            //_temp_metadata = stream.ReadBytes(totalSize);
        }

    public virtual void ConsumeDataSegments(AbstractExternalLoader[] externalData) { }
}