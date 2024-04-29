using System;
using System.Collections.Generic;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal class ExternalData
{
    public Guid TypeGuid { get; protected internal set; }

    public ExternalData()
    {
        TypeGuid = Guid.Empty;
    }

    public ExternalData(Guid typeGuid)
    {
        TypeGuid = typeGuid;
    }

    public virtual void ReadData(BinaryReader stream, IDictionary<object, object> userdata, int totalSize)
    {
        stream.BaseStream.Seek(totalSize, SeekOrigin.Current);
        //_unknownRawData = stream.ReadBytes(totalSize);
    }
}