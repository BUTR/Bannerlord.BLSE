using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.TPac
{
    internal static class TextureUtil
    {
        public static bool IsPremultiplied(this TextureFormat format)
        {
            return format is TextureFormat.DXT2 or TextureFormat.DXT4;
        }

        public static bool IsSupported(this TextureFormat format) => format switch
        {
            // in fact R16G16B16 can't be exported, too
            TextureFormat.DF24 => false,
            TextureFormat.ATOC => false,
            TextureFormat.A2M0 => false,
            TextureFormat.A2M1 => false,
            TextureFormat.BC6H_UF16 => false,
            TextureFormat.BC7 => false,
            TextureFormat.INDEX16 => false,
            TextureFormat.INDEX32 => false,
            _ => true
        };

        public static int GetAlignSize(this TextureFormat format) => format switch
        {
            TextureFormat.DXT1 => 4,
            TextureFormat.DXT2 => 4,
            TextureFormat.DXT3 => 4,
            TextureFormat.DXT4 => 4,
            TextureFormat.DXT5 => 4,
            TextureFormat.BC4 => 4,
            TextureFormat.BC5 => 4,
            TextureFormat.BC6H_UF16 => 4,
            TextureFormat.BC7 => 4,
            _ => 1
        };

        public static int GetBitsPerPixel(this TextureFormat format) => format switch
        {
            TextureFormat.R32G32B32A32F => 128,
            TextureFormat.R32G32B32A32_UINT => 128,
            TextureFormat.R32G32B32F => 96,
            TextureFormat.R32G32B32_UINT => 96,
            TextureFormat.R16G16B16A16_UNORM => 64,
            TextureFormat.R16G16B16A16F => 64,
            TextureFormat.D32_S8X24_UINT => 64,
            TextureFormat.R32G32F => 64,
            TextureFormat.R32G32_UINT => 64,
            TextureFormat.R16G16B16A16_UINT => 64,
            TextureFormat.R16G16B16 => 48,
            TextureFormat.B8G8R8A8_UNORM => 32,
            TextureFormat.B8G8R8X8_UNORM => 32,
            TextureFormat.R16G16_UNORM => 32,
            TextureFormat.R16G16F => 32,
            TextureFormat.R32_UINT => 32,
            TextureFormat.R8G8B8A8_UNORM => 32,
            TextureFormat.R8G8B8A8_UINT => 32,
            TextureFormat.D24_UNORM_S8_UINT => 32,
            TextureFormat.D24_UNORM_X8_UINT => 32,
            TextureFormat.D32F => 32,
            TextureFormat.R32F => 32,
            TextureFormat.R11G11B10F => 32,
            TextureFormat.R24G8_TYPELESS => 32,
            TextureFormat.R16G16_UINT => 32,
            TextureFormat.R8G8B8 => 24,
            TextureFormat.B8G8R8 => 24,
            TextureFormat.R16_UNORM => 16,
            TextureFormat.R16F => 16,
            TextureFormat.D16_UNORM => 16,
            TextureFormat.L16_UNORM => 16,
            TextureFormat.R16_UINT => 16,
            TextureFormat.R8G8_UNORM => 16,
            TextureFormat.A8_UNORM => 8,
            TextureFormat.L8_UNORM => 8,
            TextureFormat.R8_UNORM => 8,
            TextureFormat.DXT2 => 8,
            TextureFormat.DXT3 => 8,
            TextureFormat.DXT4 => 8,
            TextureFormat.DXT5 => 8,
            TextureFormat.BC5 => 8,
            TextureFormat.BC6H_UF16 => 8,
            TextureFormat.BC7 => 8,
            TextureFormat.R8_UINT => 8,
            TextureFormat.DXT1 => 4,
            TextureFormat.BC4 => 4,
            _ => throw new Exception("Unsupported format:" + format.ToString())
        };

        private static byte FloatToByte(float v) => (byte) MathF.Min(Math.Max(MathF.Round(v * 255f), 0), 255);

        private static void DecodeTextureDataToWriter(byte[] data, int width, int height, TextureFormat format, PipelineWriter writer, bool silentlyFail = false)
        {
            if (!format.IsSupported())
            {
                if (!silentlyFail)
                    throw new FormatException("Unsupported format: " + format);
                var byteBuffer = new byte[width * 4];
                for (var x = 0; x < width; x++)
                {
                    var i = x * 4;
                    byteBuffer[i] = 160;
                    byteBuffer[i + 1] = 160;
                    byteBuffer[i + 2] = 160;
                    byteBuffer[i + 3] = 255;
                }

                for (var y = 0; y < height; y++)
                {
                    writer.WriteLine(byteBuffer, true);
                }
            }
            else
            {
                PipelineReader reader = format switch
                {
                    TextureFormat.DXT1 => new DXT1Reader(data, format, width, height),
                    TextureFormat.DXT2 => new DXT3Reader(data, format, width, height),
                    TextureFormat.DXT3 => new DXT3Reader(data, format, width, height),
                    TextureFormat.DXT4 => new DXT5Reader(data, format, width, height),
                    TextureFormat.DXT5 => new DXT5Reader(data, format, width, height),
                    TextureFormat.BC4 => new BC4Reader(data, format, width, height),
                    TextureFormat.BC5 => new BC5Reader(data, format, width, height),
                    _ => new DefaultReader(data, format, width, height)
                };

                reader.Read(writer);
            }
        }

        public static Bitmap DecodeTextureDataToBitmap(byte[] data, int width, int height, TextureFormat format)
        {
            var bitmap = new Bitmap(width, height, format.IsPremultiplied() ? PixelFormat.Format32bppPArgb : PixelFormat.Format32bppArgb);
            BitmapData? bitmapData = null;
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                PipelineWriter writer = new ARGB32Writer(bitmapData.Scan0, bitmapData.Width, bitmapData.Height, bitmapData.Stride);
                DecodeTextureDataToWriter(data, width, height, format, writer);
            }
            finally
            {
                if (bitmapData is not null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            return bitmap;
        }

        private abstract class PipelineReader
        {
            protected readonly byte[] dataSource;
            protected readonly int width, height;
            protected readonly TextureFormat format;

            protected PipelineReader(byte[] dataSource, TextureFormat format, int width, int height)
            {
                this.dataSource = dataSource;
                this.format = format;
                this.width = width;
                this.height = height;
            }

            public abstract void Read(PipelineWriter output);
        }

        private sealed class SimpleBinaryStream
        {
            private readonly byte[] data;
            private int pointer;

            public SimpleBinaryStream(byte[] data)
            {
                this.data = data;
            }

            public byte ReadByte()
            {
                return data[pointer++];
            }

            public ushort ReadUInt16()
            {
                var b0 = data[pointer];
                var b1 = data[pointer + 1];
                pointer += 2;
                return (ushort) (b0 | b1 << 8);
            }

            public uint ReadUInt24()
            {
                var b0 = data[pointer];
                var b1 = data[pointer + 1];
                var b2 = data[pointer + 2];
                pointer += 3;
                return (uint) (b0 | b1 << 8 | b2 << 16);
            }

            public uint ReadUInt32()
            {
                var b0 = data[pointer];
                var b1 = data[pointer + 1];
                var b2 = data[pointer + 2];
                var b3 = data[pointer + 3];
                pointer += 4;
                return (uint) (b0 | b1 << 8 | b2 << 16 | b3 << 24);
            }

            public ulong ReadUInt64()
            {
                var numLSB = (uint) (data[pointer] | data[pointer + 1] << 8 | data[pointer + 2] << 16 | data[pointer + 3] << 24);
                var numMSB = (uint) (data[pointer + 4] | data[pointer + 5] << 8 | data[pointer + 6] << 16 | data[pointer + 7] << 24);
                pointer += 8;
                return (ulong) numMSB << 32 | numLSB;
            }

            public long ReadInt64()
            {
                var numLSB = (uint) (data[pointer] | data[pointer + 1] << 8 | data[pointer + 2] << 16 | data[pointer + 3] << 24);
                var numMSB = (uint) (data[pointer + 4] | data[pointer + 5] << 8 | data[pointer + 6] << 16 | data[pointer + 7] << 24);
                pointer += 8;
                return (long) ((ulong) numMSB << 32 | numLSB);
            }

            public float ReadSingle()
            {
                var f = BitConverter.ToSingle(data, pointer);
                pointer += 4;
                return f;
            }
        }

        private class DefaultReader : PipelineReader
        {
            private int bytePerPixel;

            public DefaultReader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
                bytePerPixel = format.GetBitsPerPixel() / 8;
            }

            public override void Read(PipelineWriter output)
            {
                var stream = new SimpleBinaryStream(dataSource);
                switch (format)
                {
                    case TextureFormat.A8_UNORM:
                    case TextureFormat.L8_UNORM:
                    case TextureFormat.R8_UNORM:
                    case TextureFormat.R8_UINT:
                    case TextureFormat.R8G8_UNORM:
                    case TextureFormat.R8G8B8:
                    case TextureFormat.B8G8R8:
                    case TextureFormat.B8G8R8A8_UNORM:
                    case TextureFormat.B8G8R8X8_UNORM:
                    case TextureFormat.R8G8B8A8_UNORM:
                    case TextureFormat.R8G8B8A8_UINT:
                        var byteBuffer = new byte[width * 4];
                        for (var y = 0; y < height; y++)
                        {
                            ReadLine8bpp(stream, output, byteBuffer);
                            Array.Clear(byteBuffer, 0, byteBuffer.Length);
                        }

                        byteBuffer = null;
                        break;
                    case TextureFormat.R16_UINT:
                    case TextureFormat.R16_UNORM:
                    case TextureFormat.D16_UNORM:
                    case TextureFormat.L16_UNORM:
                    case TextureFormat.R16G16_UNORM:
                    case TextureFormat.R16G16_UINT:
                    case TextureFormat.R16G16B16:
                    case TextureFormat.R16G16B16A16_UINT:
                        var ushortBuffer = new ushort[width * 4];
                        for (var y = 0; y < height; y++)
                        {
                            ReadLine16bpp(stream, output, ushortBuffer);
                            Array.Clear(ushortBuffer, 0, ushortBuffer.Length);
                        }

                        ushortBuffer = null;
                        break;
                    case TextureFormat.R32_UINT:
                    case TextureFormat.R32G32_UINT:
                    case TextureFormat.R32G32B32_UINT:
                    case TextureFormat.R32G32B32A32_UINT:
                    case TextureFormat.R24G8_TYPELESS:
                        var uintBuffer = new uint[width * 4];
                        for (var y = 0; y < height; y++)
                        {
                            ReadLine32bpp(stream, output, uintBuffer);
                            Array.Clear(uintBuffer, 0, uintBuffer.Length);
                        }

                        uintBuffer = null;
                        break;
                    case TextureFormat.D24_UNORM_S8_UINT:
                    case TextureFormat.D24_UNORM_X8_UINT:
                    case TextureFormat.D32_S8X24_UINT:
                    case TextureFormat.D32F:
                        var depthBuffer = new float[width];
                        var stencilBuffer = new byte[width];
                        for (var y = 0; y < height; y++)
                        {
                            ReadLineDepth(stream, output, depthBuffer, stencilBuffer);
                        }

                        depthBuffer = null;
                        stencilBuffer = null;
                        break;
                    case TextureFormat.R11G11B10F:
                    case TextureFormat.R16F:
                    case TextureFormat.R16G16F:
                    case TextureFormat.R16G16B16A16F:
                    case TextureFormat.R32F:
                    case TextureFormat.R32G32F:
                    case TextureFormat.R32G32B32F:
                    case TextureFormat.R32G32B32A32F:
                        var floatBuffer = new float[width * 4];
                        for (var y = 0; y < height; y++)
                        {
                            ReadLineFloat(stream, output, floatBuffer);
                            Array.Clear(floatBuffer, 0, floatBuffer.Length);
                        }

                        floatBuffer = null;
                        break;
                    default:
                        throw new Exception("Unsupported format:" + format.ToString());
                }
            }

            private void ReadLine8bpp(SimpleBinaryStream input, PipelineWriter output, byte[] buffer)
            {
                var normalized = true;
                switch (format)
                {
                    case TextureFormat.A8_UNORM:
                        for (var i = 0; i < width; i++)
                            buffer[i * 4 + 3] = input.ReadByte();
                        break;
                    case TextureFormat.L8_UNORM:
                    case TextureFormat.R8_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        break;
                    case TextureFormat.R8_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        normalized = false;
                        break;
                    case TextureFormat.R8G8_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        break;
                    case TextureFormat.R8G8B8:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        break;
                    case TextureFormat.B8G8R8:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j] = input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        break;
                    case TextureFormat.B8G8R8A8_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j] = input.ReadByte();
                            buffer[j + 3] = input.ReadByte();
                        }

                        break;
                    case TextureFormat.B8G8R8X8_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j] = input.ReadByte();
                            input.ReadByte();
                            buffer[j + 3] = byte.MaxValue;
                        }

                        break;
                    case TextureFormat.R8G8B8A8_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 3] = input.ReadByte();
                        }

                        break;
                    case TextureFormat.R8G8B8A8_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadByte();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j + 2] = input.ReadByte();
                            buffer[j + 3] = input.ReadByte();
                        }

                        normalized = false;
                        break;
                }

                output.WriteLine(buffer, normalized);
            }

            private void ReadLine16bpp(SimpleBinaryStream input, PipelineWriter output, ushort[] buffer)
            {
                var normalized = true;
                switch (format)
                {
                    case TextureFormat.R16_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 3] = ushort.MaxValue;
                        }

                        normalized = false;
                        break;
                    case TextureFormat.R16_UNORM:
                    case TextureFormat.D16_UNORM:
                    case TextureFormat.L16_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 3] = ushort.MaxValue;
                        }

                        break;
                    case TextureFormat.R16G16_UNORM:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 1] = input.ReadUInt16();
                            buffer[j + 3] = ushort.MaxValue;
                        }

                        break;
                    case TextureFormat.R16G16_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 1] = input.ReadUInt16();
                            buffer[j + 3] = ushort.MaxValue;
                        }

                        normalized = false;
                        break;
                    case TextureFormat.R16G16B16:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 1] = input.ReadUInt16();
                            buffer[j + 2] = input.ReadUInt16();
                            buffer[j + 3] = ushort.MaxValue;
                        }

                        break;
                    case TextureFormat.R16G16B16A16_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt16();
                            buffer[j + 1] = input.ReadUInt16();
                            buffer[j + 2] = input.ReadUInt16();
                            buffer[j + 3] = input.ReadUInt16();
                        }

                        break;
                }

                output.WriteLine(buffer, normalized);
            }

            private void ReadLine32bpp(SimpleBinaryStream input, PipelineWriter output, uint[] buffer)
            {
                var normalized = false;
                switch (format)
                {
                    case TextureFormat.R32_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt32();
                            buffer[j + 3] = uint.MaxValue;
                        }

                        break;
                    case TextureFormat.R32G32_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt32();
                            buffer[j + 1] = input.ReadUInt32();
                            buffer[j + 3] = uint.MaxValue;
                        }

                        break;
                    case TextureFormat.R32G32B32_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt32();
                            buffer[j + 1] = input.ReadUInt32();
                            buffer[j + 2] = input.ReadUInt32();
                            buffer[j + 3] = uint.MaxValue;
                        }

                        break;
                    case TextureFormat.R32G32B32A32_UINT:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt32();
                            buffer[j + 1] = input.ReadUInt32();
                            buffer[j + 2] = input.ReadUInt32();
                            buffer[j + 3] = input.ReadUInt32();
                        }

                        break;
                    case TextureFormat.R24G8_TYPELESS:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadUInt24();
                            buffer[j + 1] = input.ReadByte();
                            buffer[j + 3] = uint.MaxValue;
                        }

                        break;
                }

                output.WriteLine(buffer, normalized);
            }

            private void ReadLineDepth(SimpleBinaryStream input, PipelineWriter output, float[] depth, byte[] stencil)
            {
                switch (format)
                {
                    case TextureFormat.D24_UNORM_S8_UINT:
                        for (var i = 0; i < width; i++)
                        {
                            depth[i] = Int24ToSingle(input.ReadUInt24());
                            stencil[i] = input.ReadByte();
                        }

                        break;
                    case TextureFormat.D24_UNORM_X8_UINT:
                        for (var i = 0; i < width; i++)
                        {
                            depth[i] = Int24ToSingle(input.ReadUInt24());
                            input.ReadByte();
                        }

                        break;
                    case TextureFormat.D32_S8X24_UINT:
                        for (var i = 0; i < width; i++)
                        {
                            depth[i] = Int32ToSingle(input.ReadUInt32());
                            stencil[i] = input.ReadByte();
                            input.ReadUInt24();
                        }

                        break;
                    case TextureFormat.D32F:
                        for (var i = 0; i < width; i++)
                        {
                            depth[i] = input.ReadSingle();
                        }

                        break;
                }

                output.WriteLine(depth, stencil);
            }

            private void ReadLineFloat(SimpleBinaryStream input, PipelineWriter output, float[] buffer)
            {
                switch (format)
                {
                    case TextureFormat.R11G11B10F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            UnpackR11G11B10F(input.ReadUInt32(), out var r, out var g, out var b);
                            buffer[j] = r;
                            buffer[j + 1] = g;
                            buffer[j + 2] = b;
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R16F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R16G16F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 1] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R16G16B16A16F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 1] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 2] = HalfToSingle(input.ReadUInt16());
                            buffer[j + 3] = HalfToSingle(input.ReadUInt16());
                        }

                        break;
                    case TextureFormat.R32F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadSingle();
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R32G32F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadSingle();
                            buffer[j + 1] = input.ReadSingle();
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R32G32B32F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadSingle();
                            buffer[j + 1] = input.ReadSingle();
                            buffer[j + 2] = input.ReadSingle();
                            buffer[j + 3] = 1f;
                        }

                        break;
                    case TextureFormat.R32G32B32A32F:
                        for (int i = 0, j = 0; i < width; i++, j = i * 4)
                        {
                            buffer[j] = input.ReadSingle();
                            buffer[j + 1] = input.ReadSingle();
                            buffer[j + 2] = input.ReadSingle();
                            buffer[j + 3] = input.ReadSingle();
                        }

                        break;
                }

                output.WriteLine(buffer);
            }

            private static float Int24ToSingle(uint value)
            {
                return (float) (value / 16777215d);
            }

            private static float Int32ToSingle(uint value)
            {
                return (float) (value / (double) UInt32.MaxValue);
            }

            private static float HalfToSingle(ushort value)
            {
                return SystemHalf.Half.ToHalf(value);
            }

            private static void UnpackR11G11B10F(uint value, out float r, out float g, out float b)
            {
                // https://github.com/anonymousguy198/swiftshader-compiled/blob/a6bc61d61d6fe9551d72f917629bf6bccfeafce0/src/Common/Half.hpp#L73
                var bits = (value & 0x7FFu) << 4;
                r = HalfToSingle((ushort) bits);
                bits = ((value >> 11) & 0x7FFu) << 4;
                g = HalfToSingle((ushort) bits);
                bits = ((value >> 22) & 0x3FFu) << 5;
                b = HalfToSingle((ushort) bits);
            }
        }

        private struct RGB565
        {
            internal readonly byte R;
            internal readonly byte G;
            internal readonly byte B;
            internal readonly byte A;

            public RGB565(ushort value) : this(value, byte.MaxValue)
            {
            }

            public RGB565(ushort value, byte alpha)
            {
                var r = (value >> 11) & 0x1F;
                var g = (value >> 5) & 0x3F;
                var b = value & 0x1F;
                r = r << 3 | r >> 2;
                g = g << 2 | g >> 3;
                b = b << 3 | b >> 2;
                R = (byte) r;
                G = (byte) g;
                B = (byte) b;
                A = alpha;
            }

            private RGB565(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }

            public void WriteToBytes(byte[] rgba8, int offset)
            {
                rgba8[offset] = R;
                rgba8[offset + 1] = G;
                rgba8[offset + 2] = B;
                rgba8[offset + 3] = A;
            }

            // return (left * 2 + right) / 3 (ignore alpha)
            public static RGB565 MAD(RGB565 left, RGB565 right)
            {
                var r = (((left.R << 1) + right.R) / 3) & 0xFF;
                var g = (((left.G << 1) + right.G) / 3) & 0xFF;
                var b = (((left.B << 1) + right.B) / 3) & 0xFF;
                return new RGB565((byte) r, (byte) g, (byte) b, left.A);
            }

            // return (left + right) / 2 (ignore alpha)
            public static RGB565 AVG(RGB565 left, RGB565 right)
            {
                var r = ((left.R + right.R) >> 1) & 0xFF;
                var g = ((left.G + right.G) >> 1) & 0xFF;
                var b = ((left.B + right.B) >> 1) & 0xFF;
                return new RGB565((byte) r, (byte) g, (byte) b, left.A);
            }
        }

        private abstract class BlockReader : PipelineReader
        {
            private int bytesPerBlock;

            public BlockReader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
                bytesPerBlock = (format.GetBitsPerPixel() * 16) / 8;
            }

            public override void Read(PipelineWriter output)
            {
                var blockWidth = Math.Max((width + 3) / 4, 1);
                var blockHeight = Math.Max((height + 3) / 4, 1);
                var cache0 = new byte[4][];
                for (var i = 0; i < 4; i++)
                    cache0[i] = new byte[blockWidth * 4 * 4];
                var cache1 = new byte[4][];
                for (var i = 0; i < 4; i++)
                    cache1[i] = new byte[blockWidth * 4 * 4];
                var isPingPong = false;

                var stream = new SimpleBinaryStream(dataSource);
                for (var y = 0; y < blockHeight; y++)
                {
                    if (isPingPong)
                    {
                        for (var x = 0; x < blockWidth; x++)
                            ReadBlock(stream, cache1, x);
                    }
                    else
                    {
                        for (var x = 0; x < blockWidth; x++)
                            ReadBlock(stream, cache0, x);
                    }

                    var pingPoing = isPingPong;
                    var baseY = y;
                    for (var y2 = 0; y2 < 4; y2++)
                    {
                        var currentY = baseY * 4 + y2;
                        if (currentY < height)
                        {
                            if (pingPoing)
                            {
                                output.WriteLine(cache1[y2], false);
                            }
                            else
                            {
                                output.WriteLine(cache0[y2], false);
                            }
                        }
                    }
                    isPingPong = !isPingPong;
                }
            }

            /*protected static PixelColor GetColorFromRGB565(ushort rgb565)
            {
                int b = rgb565 & 0x1F;
                int g = (rgb565 & 0x7E0) >> 5;
                int r = (rgb565 & 0xF800) >> 11;
                b = b << 3 | b >> 2;
                g = g << 2 | g >> 3;
                r = r << 3 | r >> 2;
                return new PixelColor(r / 255f, g / 255f, b / 255f, 1f);
            }*/

            protected abstract void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX);
        }

        private class DXT1Reader : BlockReader
        {
            public DXT1Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
            }

            protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
            {
                var posX = blockX * 4;

                var baseColor = new RGB565[4];
                ushort color0, color1;
                baseColor[0] = new RGB565(color0 = stream.ReadUInt16());
                baseColor[1] = new RGB565(color1 = stream.ReadUInt16());
                if (color0 > color1)
                {
                    // baseColor[2] = (baseColor[0] * 2 + baseColor[1]) / 3;
                    baseColor[2] = RGB565.MAD(baseColor[0], baseColor[1]);
                    baseColor[3] = RGB565.MAD(baseColor[1], baseColor[0]);
                }
                else
                {
                    baseColor[2] = RGB565.AVG(baseColor[0], baseColor[1]);
                    baseColor[3] = new RGB565(0, byte.MaxValue);
                }

                for (var y = 0; y < 4; y++)
                {
                    var writeLine = cache[y];
                    var index = stream.ReadByte();
                    baseColor[(index >> 0) & 0x3].WriteToBytes(writeLine, (posX + 0) * 4);
                    baseColor[(index >> 2) & 0x3].WriteToBytes(writeLine, (posX + 1) * 4);
                    baseColor[(index >> 4) & 0x3].WriteToBytes(writeLine, (posX + 2) * 4);
                    baseColor[(index >> 6) & 0x3].WriteToBytes(writeLine, (posX + 3) * 4);
                }
            }
        }
        private class DXT3Reader : DXT1Reader
        {
            public DXT3Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
            }

            protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
            {
                var alphaData = stream.ReadUInt64();
                base.ReadBlock(stream, cache, blockX);
                var posX = blockX * 4;
                var i = 0;
                for (var y = 0; y < 4; y++)
                {
                    var writeLine = cache[y];
                    for (var x = 0; x < 4; x++)
                    {
                        var alpha = (int) ((alphaData >> (i * 4)) & 0xF);
                        i++;
                        alpha = (alpha << 4 | alpha) & 0xFF;
                        writeLine[(posX + x) * 4 + 3] = (byte) alpha;
                    }
                }
            }
        }
        private class DXT5Reader : DXT1Reader
        {
            public DXT5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
            }

            protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
            {
                var alphaData = stream.ReadInt64();
                base.ReadBlock(stream, cache, blockX);
                ReadBC3AlphaBlock(alphaData, cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
                    out byte addR, out byte addG, out byte addB, out byte addA) =>
                {
                    maskR = maskG = maskB = 0;
                    maskA = byte.MaxValue;
                    addR = addG = addB = addA = 0;
                });
                /*ReadBC3AlphaBlock(alphaData, cache, blockX * 4, (ref PixelColor pixel, float value) =>
                {
                    pixel.A = value;
                });*/
            }

            public delegate void GetPixelMask(out int maskR, out int maskG, out int maskB, out int maskA,
                out byte addR, out byte addG, out byte addB, out byte addA);

            public static void ReadBC3AlphaBlock(long block, byte[][] cache, int posX, GetPixelMask writeMask)
            {
                writeMask(out var maskR, out var maskG, out var maskB, out var maskA,
                    out var addR, out var addG, out var addB, out var addA);
                var alpha0 = (int) (block & 0xFF);
                var alpha1 = (int) ((block >> 8) & 0xFF);
                var isFirstGreater = alpha0 > alpha1;
                block = block >> 16;
                var alphaLookup = new byte[8];
                for (var j = 0; j < 8; j++)
                {
                    alphaLookup[j] = (byte) BC3GradientInterpolate(j, alpha0, alpha1, isFirstGreater);
                }

                var i = 0;
                for (var y = 0; y < 4; y++)
                {
                    var writeLine = cache[y];
                    for (var x = 0; x < 4; x++)
                    {
                        var alphaIndex = (int) (block >> (i * 3)) & 0x7;
                        var value = alphaLookup[alphaIndex];
                        i++;
                        var writePos = (posX + x) * 4;
                        writeLine[writePos + 0] = (byte) (((value & maskR) + (writeLine[writePos + 0] & ~maskR)) + addR);
                        writeLine[writePos + 1] = (byte) (((value & maskG) + (writeLine[writePos + 1] & ~maskG)) + addG);
                        writeLine[writePos + 2] = (byte) (((value & maskB) + (writeLine[writePos + 2] & ~maskB)) + addB);
                        writeLine[writePos + 3] = (byte) (((value & maskA) + (writeLine[writePos + 3] & ~maskA)) + addA);
                    }
                }
            }

            public static int BC3GradientInterpolate(int index, int alpha0, int alpha1, bool isFirstGreater)
            {
                if (isFirstGreater)
                {
                    switch (index)
                    {
                        case 0:
                            return alpha0;
                        case 1:
                            return alpha1;
                        case 2:
                            return (alpha0 * 6 + alpha1 * 1) / 7;
                        case 3:
                            return (alpha0 * 5 + alpha1 * 2) / 7;
                        case 4:
                            return (alpha0 * 4 + alpha1 * 3) / 7;
                        case 5:
                            return (alpha0 * 3 + alpha1 * 4) / 7;
                        case 6:
                            return (alpha0 * 2 + alpha1 * 5) / 7;
                        case 7:
                            return (alpha0 * 1 + alpha1 * 6) / 7;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            return alpha0;
                        case 1:
                            return alpha1;
                        case 2:
                            return (alpha0 * 4 + alpha1 * 1) / 5;
                        case 3:
                            return (alpha0 * 3 + alpha1 * 2) / 5;
                        case 4:
                            return (alpha0 * 2 + alpha1 * 3) / 5;
                        case 5:
                            return (alpha0 * 1 + alpha1 * 4) / 5;
                        case 6:
                            return 0;
                        case 7:
                            return ushort.MaxValue;
                    }
                }

                throw new IndexOutOfRangeException("Bad alpha index: " + index);
            }
        }
        private class BC4Reader : BlockReader
        {
            public BC4Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
            }

            protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
            {
                DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
                    out byte addR, out byte addG, out byte addB, out byte addA) =>
                {
                    maskR = byte.MaxValue;
                    maskG = maskB = maskA = 0;
                    addR = addG = addB = 0;
                    addA = byte.MaxValue;
                });
                /*DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
                {
                    pixel.R = value;
                    pixel.A = 1f;
                });*/
            }
        }
        private class BC5Reader : BC4Reader
        {
            public BC5Reader(byte[] dataSource, TextureFormat format, int width, int height) : base(dataSource, format, width, height)
            {
            }

            protected override void ReadBlock(SimpleBinaryStream stream, byte[][] cache, int blockX)
            {
                base.ReadBlock(stream, cache, blockX);
                DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), cache, blockX * 4, (out int maskR, out int maskG, out int maskB, out int maskA,
                    out byte addR, out byte addG, out byte addB, out byte addA) =>
                {
                    maskG = byte.MaxValue;
                    maskR = maskB = maskA = 0;
                    addR = addG = addB = addA = 0;
                });
                /*DXT5Reader.ReadBC3AlphaBlock(stream.ReadInt64(), ref cache, blockX * 4, (ref PixelColor pixel, float value) =>
                {
                    pixel.G = value;
                });*/
            }
        }


        private abstract class PipelineWriter
        {
            public abstract void WriteLine(byte[] rgba8, bool normalized);

            public abstract void WriteLine(ushort[] rgba16, bool normalized);

            public abstract void WriteLine(uint[] rgba32, bool normalized);

            public abstract void WriteLine(float[] depth, byte[] stencil);

            public abstract void WriteLine(float[] rgba32f);
        }
        private sealed class ARGB32Writer : PipelineWriter
        {
            private readonly int lineLimiter;
            private readonly int stride;
            private readonly int[] buffer;
            private IntPtr data;

            public ARGB32Writer(IntPtr ptr, int width, int height, int stride)
            {
                data = ptr;
                this.stride = stride;
                lineLimiter = width;
                buffer = new int[lineLimiter];
            }

            public override void WriteLine(byte[] rgba8, bool normalized)
            {
                for (var i = 0; i < lineLimiter; i++)
                {
                    var num = i * 4;
                    buffer[i] = (int) rgba8[num + 2] | ((int) rgba8[num + 1] << 8) | ((int) rgba8[num] << 16) | ((int) rgba8[num + 3] << 24);
                }
                Marshal.Copy(buffer, 0, data, lineLimiter);
                data = IntPtr.Add(data, stride);
            }

            public override void WriteLine(ushort[] rgba16, bool normalized)
            {
                for (int i = 0; i < lineLimiter; i++)
                {
                    int num = i * 4;
                    buffer[i] = (rgba16[num + 2] >> 8) | (rgba16[num + 1] >> 8 << 8) | (rgba16[num] >> 8 << 16) | (rgba16[num + 3] >> 8 << 24);
                }
                Marshal.Copy(buffer, 0, data, lineLimiter);
                data = IntPtr.Add(data, stride);
            }

            public override void WriteLine(uint[] rgba32, bool normalized)
            {
                for (int i = 0; i < lineLimiter; i++)
                {
                    int num = i * 4;
                    buffer[i] = (int) ((rgba32[num + 2] >> 24) | (rgba32[num + 1] >> 24 << 8) | (rgba32[num] >> 24 << 16) | (rgba32[num + 3] >> 24 << 24));
                }
                Marshal.Copy(buffer, 0, data, lineLimiter);
                data = IntPtr.Add(data, stride);
            }

            public override void WriteLine(float[] depth, byte[] stencil)
            {
                for (int i = 0; i < lineLimiter; i++)
                {
                    buffer[i] = 0 | ((int) stencil[i] << 8) | ((int) FloatToByte(depth[i]) << 16) | 255;
                }
                Marshal.Copy(buffer, 0, data, lineLimiter);
                data = IntPtr.Add(data, stride);
            }

            public override void WriteLine(float[] rgba32f)
            {
                for (int i = 0; i < lineLimiter; i++)
                {
                    int num = i * 4;
                    buffer[i] = (int) FloatToByte(rgba32f[num + 2]) | ((int) FloatToByte(rgba32f[num + 1]) << 8) | ((int) FloatToByte(rgba32f[num]) << 16) | ((int) FloatToByte(rgba32f[num + 3]) << 24);
                }
                Marshal.Copy(buffer, 0, data, lineLimiter);
                data = IntPtr.Add(data, stride);
            }
        }
    }
}