using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace Bannerlord.BLSE.Tests;

/// <summary>
/// Writes an empty strong-named assembly. Signs in managed code, as Roslyn does, because
/// Reflection.Emit goes through the native strong-name API, which needs a CSP key container
/// that isn't always accessible.
/// </summary>
internal static class StrongNamedAssemblyWriter
{
    private const int CALG_RSA_SIGN = 0x00002400;
    private const int CALG_SHA1 = 0x00008004;

    public static void Write(string path, string simpleName, Version version, RSACryptoServiceProvider key, string culture = "")
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString($"{simpleName}.dll"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        metadata.AddAssembly(
            metadata.GetOrAddString(simpleName),
            version,
            culture.Length > 0 ? metadata.GetOrAddString(culture) : default,
            metadata.GetOrAddBlob(GetPublicKey(key)),
            AssemblyFlags.PublicKey,
            AssemblyHashAlgorithm.Sha1);
        metadata.AddTypeDefinition(
            default,
            default,
            metadata.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        var peBuilder = new ManagedPEBuilder(
            new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll | Characteristics.ExecutableImage),
            new MetadataRootBuilder(metadata),
            new BlobBuilder(),
            flags: CorFlags.ILOnly | CorFlags.StrongNameSigned,
            strongNameSignatureSize: key.KeySize / 8);

        var image = new BlobBuilder();
        peBuilder.Serialize(image);
        peBuilder.Sign(image, content => Sign(content, key));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, image.ToArray());
    }

    /// <summary>The assembly's public key: signature and hash algorithm ids, then the CSP public key blob.</summary>
    private static byte[] GetPublicKey(RSACryptoServiceProvider key)
    {
        var cspBlob = key.ExportCspBlob(false);
        var publicKey = new byte[12 + cspBlob.Length];
        BitConverter.GetBytes(CALG_RSA_SIGN).CopyTo(publicKey, 0);
        BitConverter.GetBytes(CALG_SHA1).CopyTo(publicKey, 4);
        BitConverter.GetBytes(cspBlob.Length).CopyTo(publicKey, 8);
        cspBlob.CopyTo(publicKey, 12);
        return publicKey;
    }

    private static byte[] Sign(IEnumerable<Blob> content, RSACryptoServiceProvider key)
    {
        using var sha1 = SHA1.Create();
        foreach (var blob in content)
        {
            var segment = blob.GetBytes();
            sha1.TransformBlock(segment.Array!, segment.Offset, segment.Count, null, 0);
        }
        sha1.TransformFinalBlock([], 0, 0);

        var signature = key.SignHash(sha1.Hash, CryptoConfig.MapNameToOID("SHA1"));
        // The strong-name signature is stored little-endian
        Array.Reverse(signature);
        return signature;
    }
}
