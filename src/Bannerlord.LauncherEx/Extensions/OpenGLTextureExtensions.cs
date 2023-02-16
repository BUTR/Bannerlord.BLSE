using Bannerlord.LauncherEx.TPac;

using HarmonyLib.BUTR.Extensions;

using StbSharp;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using TaleWorlds.TwoDimension.Standalone;

namespace Bannerlord.LauncherEx.Extensions
{
    internal static class OpenGLTextureExtensions
    {
        private enum PixelFormat : uint
        {
            ColorIndex = 6400U,
            StencilIndex,
            DepthComponent,
            Red,
            Green,
            Blue,
            Alpha,
            RGB,
            RGBA,
            Luminance,
            LuminanceAlpha,
            BGR = 32992U,
            BGRA
        }

        private delegate void MakeActiveDelegate(OpenGLTexture texture);
        private static readonly MakeActiveDelegate? _makeActiveDelegate = AccessTools2.GetDelegate<MakeActiveDelegate>(typeof(OpenGLTexture), "MakeActive");

        [DllImport("Opengl32.dll", EntryPoint = "glTexImage2D")]
        private static extern void TexImage2D(uint target, int level, uint pixelInternalformat, int width, int height, int border, PixelFormat format, uint type, byte[] pixels);

        [DllImport("Opengl32.dll", EntryPoint = "glTexImage2D")]
        private static extern void TexImage2D2(uint target, int level, uint pixelInternalformat, int width, int height, int border, PixelFormat format, uint type, IntPtr pixels);

        public static void MakeActive(this OpenGLTexture texture) => _makeActiveDelegate?.Invoke(texture);

        public static bool LoadFromStream(this OpenGLTexture texture, string name, Stream stream)
        {
            if (_makeActiveDelegate is null)
                return false;

            var image = new ImageReader().Read(stream);
            texture.Initialize(name, image.Width, image.Height);
            texture.MakeActive();
            var (error, pixelFormat, pixelInternalformat) = image.Comp switch
            {
                1 => (false, PixelFormat.Red, 0x8229U),
                3 => (false, PixelFormat.RGB, 0x8051U),
                4 => (false, PixelFormat.RGBA, 0x8058U),
                _ => (true, (PixelFormat) 0, 0U),
            };
            if (!error)
            {
                TexImage2D(0x00000DE1, 0, pixelInternalformat, image.Width, image.Height, 0, pixelFormat, 0x00001401, image.Data);
            }

            return true;
        }

        public static bool LoadFromAssetTexture(this OpenGLTexture texture, string name, Texture source)
        {
            if (_makeActiveDelegate is null)
                return false;

            if (source.TexturePixels is null)
                return false;

            var textureData = source.TexturePixels.GetData();
            using var bitmap = TextureUtil.DecodeTextureDataToBitmap(textureData.PrimaryRawImage, (int) source.Width, (int) source.Height, source.Format);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            texture.Initialize(name, (int) source.Width, (int) source.Height);
            texture.MakeActive();
            TexImage2D2(0x00000DE1, 0, 0x8058U, data.Width, data.Height, 0, PixelFormat.BGRA, 0x00001401, data.Scan0);

            bitmap.UnlockBits(data);

            return true;
        }
    }
}