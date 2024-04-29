using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bannerlord.LauncherEx.TPac;

internal static class Utils
{
    public static string ReadSizedString(this BinaryReader stream)
    {
        var num = stream.ReadInt32();
        if (num == 0)
        {
            return string.Empty;
        }

        var array = stream.ReadBytes(num);
        return Encoding.UTF8.GetString(array);
    }

    public static List<string> ReadStringList(this BinaryReader stream)
    {
        var num = stream.ReadInt32();
        var list = new List<string>(num);
        for (var i = 0; i < num; i++)
        {
            list.Add(stream.ReadSizedString());
        }
        return list;
    }

    public static Guid ReadGuid(this BinaryReader stream) => new Guid(stream.ReadBytes(16));

    public static BinaryReader OpenBinaryReader(this FileInfo file) => new(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));

    public static BinaryReader CreateBinaryReader(this byte[] data) => new(new MemoryStream(data, false));
}