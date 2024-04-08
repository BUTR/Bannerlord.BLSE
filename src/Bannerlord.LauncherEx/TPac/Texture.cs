using System;
using System.Collections.Generic;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal class Texture : AssetItem
{
    public static readonly Guid TYPE_GUID = Guid.Parse("c974cbcb-5f1c-49f6-9a32-2b5b6c92c2e8");


    public List<string> Flags { set; get; } = new();
    /*
     dont_degrade 0x2
     dont_delay_loading 0x80
     is_envmap 0x1000000
     is_specularmap 0x800000
     is_bumpmap 0x400000
     for_terrain 0x200000
     for_colorgrade 0x100000
     for_skybox_background 0x4000000
     for_skybox_cloud 0x8000000
     for_skybox_sun 0x10000000
     dont_compress 0x20
     ignore_alpha 0x80000
     dont_resize_in_atlas 0x100
     */

    public string Source { set; get; }
    public uint Width { set; get; }
    public uint Height { set; get; }
    public byte MipmapCount { set; get; }
    public ushort ArrayCount { set; get; }
    public TextureFormat Format { set; get; }

    private string? _rawFormat;

    public List<string> SystemFlags { set; get; } = new();
    /*
     has_alpha 0x8000
     is_cubemap 0x2000
     */

    public ExternalLoader<TexturePixelData>? TexturePixels { set; get; }

    public bool HasPixelData => TexturePixels is not null;

    public Texture() : base(TYPE_GUID)
    {
        Source = string.Empty;
    }

    public override void ReadMetadata(BinaryReader stream, int totalSize)
    {
        var pos = stream.BaseStream.Position;
        var version = stream.ReadUInt32();
        stream.ReadGuid();
        var UnknownUint1 = stream.ReadUInt32();
        Source = stream.ReadSizedString();
        var UnknownUlong = stream.ReadUInt64();
        var UnknownBool = stream.ReadBoolean();
        var UnknownUint2 = stream.ReadUInt32();
        Flags = stream.ReadStringList();
        var UnknownUint3 = stream.ReadUInt32();

        var UnknownByte = stream.ReadByte();
        Width = stream.ReadUInt32();
        Height = stream.ReadUInt32();
        var UnknownUint4 = stream.ReadUInt32();
        MipmapCount = stream.ReadByte();
        ArrayCount = stream.ReadUInt16();
        _rawFormat = stream.ReadSizedString();
        Format = Enum.TryParse(_rawFormat, true, out TextureFormat format) ? format : TextureFormat.UNKNOWN;

        var UnknownUint5 = stream.ReadUInt32();
        SystemFlags = stream.ReadStringList();

        if (UnknownUint3 > 0)
        {
            var UnknownUint6 = stream.ReadUInt32();
            var UnknownUint7 = stream.ReadUInt32();
        }

        // dirty hack for 1.5.0
        // TW introduced a new field for the metadata of texture since 1.5.0
        // but they didn't bump the version of metadata
        if (version >= 1 || totalSize - (stream.BaseStream.Position - pos) == 4)
        {
            var numPair = stream.ReadUInt32();
            for (var i = 0; i < numPair; i++)
            {
                stream.ReadGuid();
                stream.ReadGuid();
            }
        }

        if (version >= 2)
        {
            var UnknownUlong2 = stream.ReadUInt64();
        }
    }

    public override void ConsumeDataSegments(AbstractExternalLoader[] externalData)
    {
        foreach (var externalLoader in externalData)
        {
            if (externalLoader is ExternalLoader<TexturePixelData> pixelData)
            {
                var ud = pixelData.UserData;
                ud[TexturePixelData.KEY_WIDTH] = (int) Width;
                ud[TexturePixelData.KEY_HEIGHT] = (int) Height;
                ud[TexturePixelData.KEY_ARRAY] = (int) ArrayCount;
                ud[TexturePixelData.KEY_MIPMAP] = (int) MipmapCount;
                ud[TexturePixelData.KEY_FORMAT] = Format;
                TexturePixels = pixelData;
            }
            /*else if (externalLoader is ExternalLoader<TextureImportSettingsData> importSetting)
            {
            }*/
        }

        base.ConsumeDataSegments(externalData);
    }
}