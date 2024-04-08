using System;
using System.Collections.Generic;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal class TexturePixelData : ExternalData
{
    public static readonly Guid TYPE_GUID = Guid.Parse("70ee4e2c-79e4-4b2d-8d54-d53ecd2a559c");

    public const string KEY_WIDTH = "width";

    public const string KEY_HEIGHT = "height";

    public const string KEY_ARRAY = "array";

    public const string KEY_MIPMAP = "mipmap";

    public const string KEY_FORMAT = "format";

    /*public const string KEY_PIXELSIZE = "pixelSize";
    public const string KEY_ALIGN = "align";*/

    public byte[][][] RawImage { set; get; } = Array.Empty<byte[][]>();

    public byte[] PrimaryRawImage => RawImage[0][0];

    public TexturePixelData() : base(TYPE_GUID) { }

    public override void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
    {
            int array = 1, mipmap = 1;
            if (userdata.TryGetValue(KEY_ARRAY, out var arrayObj))
            {
                array = (int) arrayObj;
            }

            if (userdata.TryGetValue(KEY_MIPMAP, out var mipmapObj))
            {
                mipmap = (int) mipmapObj;
            }

            if (!userdata.TryGetValue(KEY_WIDTH, out var widthObj) || !userdata.TryGetValue(KEY_HEIGHT, out var heightObj) || !userdata.TryGetValue(KEY_FORMAT, out var formatObj))
            {
                throw new ArgumentException("No enough user data for width, height and format");
            }

            var width = (int) widthObj;
            var height = (int) heightObj;
            var format = (TextureFormat) formatObj;
            var aligh = format.GetAlignSize();
            var pixelSize = format.GetBitsPerPixel();

            //var raw = stream.ReadBytes(totalSize);

            var raw = new byte[array][][];
            for (var a = 0; a < array; a++)
            {
                raw[a] = new byte[mipmap][];
                var imageWidth = width;
                var imageHeight = height;
                for (var m = 0; m < mipmap; m++)
                {
                    var alignedWidth = imageWidth;
                    var alignedHeight = imageHeight;
                    if (alignedWidth % aligh != 0)
                    {
                        alignedWidth += aligh - (alignedWidth % aligh);
                    }

                    if (alignedHeight % aligh != 0)
                    {
                        alignedHeight += aligh - (alignedHeight % aligh);
                    }

                    // prevent from overflow for mega textures or underflow for very small mipmap
                    // should be alignedWidth * alignedHeight * pixelSize / 8
                    var readSize = alignedWidth * alignedHeight;
                    // if readSize & 7 != 0, then it cannot be divided exactly
                    if (readSize >= 8 && (readSize & 7) == 0)
                        readSize = readSize / 8 * pixelSize;
                    else
                        readSize = readSize * pixelSize / 8;
                    raw[a][m] = stream.ReadBytes(readSize);
                    imageWidth = Math.Max(imageWidth >> 1, 1);
                    imageHeight = Math.Max(imageHeight >> 1, 1);
                }
            }

            RawImage = raw;
        }
}