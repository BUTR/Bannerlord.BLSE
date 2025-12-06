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

    // Updated code taken from https://github.com/hunharibo/TpacTool
    public override void ReadMetadata(BinaryReader stream, int totalSize)
    {
        var startPos = stream.BaseStream.Position;
        
        var version = stream.ReadUInt32();
        var BillboardMaterial = stream.ReadGuid();
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
        
        var currentPos = stream.BaseStream.Position;
        var bytesRead = currentPos - startPos;
        var remaining = totalSize - bytesRead;

        // Check if we should read generated assets
        if (version >= 1 || remaining == 4)
        {
            var numPair = stream.ReadUInt32();
            for (var i = 0; i < numPair; i++)
            {
                stream.ReadGuid();
                stream.ReadGuid();
            }
        }

        // Handle version 2 and 3 fields
        if (version >= 2)
        {
            var UnknownUlong2 = stream.ReadUInt64();
            remaining -= 8;
        }
        
        if (version >= 3)
        {
            stream.ReadBytes(32);
            remaining -= 32;
        }
        
        // Ensure we consume exactly totalSize bytes by reading any remaining data
        currentPos = stream.BaseStream.Position;
        bytesRead = currentPos - startPos;
        remaining = totalSize - bytesRead;

        if (remaining > 0)
        {
            stream.ReadBytes((int)remaining);
        }
        else if (remaining < 0)
        {
#if DEBUG
            // This indicates we read too much - this is a serious error
            throw new InvalidDataException($"Texture metadata read {-remaining} bytes beyond expected size. " +
                                           $"Asset: {Name}, Expected: {totalSize}, Position: {currentPos}, Start: {startPos}");
#endif
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