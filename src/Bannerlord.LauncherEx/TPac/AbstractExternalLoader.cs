using System;
using System.Collections.Generic;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal abstract class AbstractExternalLoader
{
    protected static readonly IDictionary<object, object> EMPTY_USERDATA = new Dictionary<object, object>();

    protected FileInfo? _file;

    protected internal ulong _offset;

    protected internal ulong _actualSize;

    protected internal ulong _storageSize;

    protected internal StorageFormat _storageFormat;

    public Guid TypeGuid { internal set; get; }

    public Guid OwnerGuid { set; get; }

    protected readonly Lazy<Dictionary<object, object>> _userdata = new();

    public IDictionary<object, object> UserData => _userdata.Value;

    public enum StorageFormat : byte
    {
        Uncompressed = 0,
        LZ4HC = 1
    }
}