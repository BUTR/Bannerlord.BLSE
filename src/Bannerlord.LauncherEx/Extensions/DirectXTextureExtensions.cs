#if v151
using Bannerlord.LauncherEx.ResourceManagers;
using Bannerlord.LauncherEx.TPac;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using StbSharp;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using TaleWorlds.TwoDimension.Standalone;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace Bannerlord.LauncherEx.Extensions;

// Since v1.5.1 the launcher renderer is DirectX 11 based. DirectXTexture has no public API for
// uploading raw pixel data and its own file loader rejects grayscale+alpha (comp 2) PNGs, which
// is what our icon textures decode to. So we decode with StbSharp forcing RGBA (like the old
// OpenGL path did), create the D3D11 texture through the game's public native wrappers and fill
// the private DirectXTexture fields.
internal static class DirectXTextureExtensions
{
    private const uint DXGI_FORMAT_R8G8B8A8_UNORM = 28;
    private const uint DXGI_FORMAT_B8G8R8A8_UNORM = 87;
    private const uint D3D11_BIND_SHADER_RESOURCE = 8;

    private static readonly AccessTools.FieldRef<DirectXTexture, int>? _width = AccessTools2.FieldRefAccess<DirectXTexture, int>("_width");
    private static readonly AccessTools.FieldRef<DirectXTexture, int>? _height = AccessTools2.FieldRefAccess<DirectXTexture, int>("_height");
    private static readonly AccessTools.FieldRef<DirectXTexture, string>? _name = AccessTools2.FieldRefAccess<DirectXTexture, string>("_name");
    private static readonly AccessTools.FieldRef<DirectXTexture, IntPtr>? _device = AccessTools2.FieldRefAccess<DirectXTexture, IntPtr>("_device");
    private static readonly AccessTools.FieldRef<DirectXTexture, IntPtr>? _texture = AccessTools2.FieldRefAccess<DirectXTexture, IntPtr>("_texture");
    private static readonly AccessTools.FieldRef<DirectXTexture, IntPtr>? _srv = AccessTools2.FieldRefAccess<DirectXTexture, IntPtr>("_srv");

    public static bool LoadFromStream(this DirectXTexture texture, string name, Stream stream)
    {
        try
        {
            var image = new ImageReader().Read(stream, 4);
            return texture.LoadFromRawPixels(name, image.Data, image.Width, image.Height, DXGI_FORMAT_R8G8B8A8_UNORM, (uint) (image.Width * 4));
        }
        catch
        {
            return false;
        }
    }

    public static bool LoadFromAssetTexture(this DirectXTexture texture, string name, Texture source)
    {
        if (source.TexturePixels is null)
            return false;

        var textureData = source.TexturePixels.GetData();
        using var bitmap = TextureUtil.DecodeTextureDataToBitmap(textureData.PrimaryRawImage, (int) source.Width, (int) source.Height, source.Format);
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        try
        {
            var pixels = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            return texture.LoadFromRawPixels(name, pixels, data.Width, data.Height, DXGI_FORMAT_B8G8R8A8_UNORM, (uint) data.Stride);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static bool LoadFromRawPixels(this DirectXTexture texture, string name, byte[] pixels, int width, int height, uint dxgiFormat, uint pitch)
    {
        if (_width is null || _height is null || _name is null || _device is null || _texture is null || _srv is null)
            return false;

        if (GraphicsContextManager.Instance is not { } weakRef || !weakRef.TryGetTarget(out var gc) || gc is null)
            return false;

        var device = gc.DeviceHandle;
        var desc = new D3D11_TEXTURE2D_DESC
        {
            Width = (uint) width,
            Height = (uint) height,
            MipLevels = 1u,
            ArraySize = 1u,
            Format = dxgiFormat,
            SampleDesc = new DXGI_SAMPLE_DESC
            {
                Count = 1u,
                Quality = 0u,
            },
            Usage = 0u,
            BindFlags = D3D11_BIND_SHADER_RESOURCE,
            CPUAccessFlags = 0u,
            MiscFlags = 0u,
        };

        var tex = IntPtr.Zero;
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            var initialData = new D3D11_SUBRESOURCE_DATA
            {
                pSysMem = handle.AddrOfPinnedObject(),
                SysMemPitch = pitch,
                SysMemSlicePitch = 0u,
            };
            if (D3D11Device.CreateTexture2D(device, ref desc, ref initialData, out tex) < 0)
                return false;
        }
        finally
        {
            handle.Free();
        }

        if (D3D11Device.CreateShaderResourceView(device, tex, out var srv) < 0)
        {
            ComRelease.Release(tex);
            return false;
        }

        _width(texture) = width;
        _height(texture) = height;
        _name(texture) = name;
        _device(texture) = device;
        _texture(texture) = tex;
        _srv(texture) = srv;
        return true;
    }
}
#endif
