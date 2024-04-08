using LZ4;

using System;
using System.IO;

namespace Bannerlord.LauncherEx.TPac;

internal sealed class ExternalLoader<T> : AbstractExternalLoader where T : ExternalData, new()
{
    private const int IMMEDIATELY_GC_THRESHOLD = 128 * 1024 * 1024 - 1;

    internal ExternalLoader(FileInfo file)
    {
            _file = file;
            if (!_file.Exists) throw new FileNotFoundException("Cannot find file: " + _file.FullName);
            OwnerGuid = Guid.Empty;
        }

    public ExternalLoader() : this(new T()) { }

    public ExternalLoader(T data)
    {
            _file = null;
            //OwnerGuid = Guid.NewGuid(); // sz: don't assign a random guid for it makes no sense
            OwnerGuid = Guid.Empty;
            if (data.TypeGuid == Guid.Empty)
            {
                throw new ArgumentException("The data which were assigned to the loader must have a type guid.");
            }

            TypeGuid = data.TypeGuid;
        }

    private T ReadData()
    {
            if (_file is null) throw new InvalidOperationException("Can't read data from file");
            using var stream = _file.OpenBinaryReader();
            return ReadData(stream);
        }

    private T ReadData(BinaryReader fullStream)
    {
            var rawData = GetRawData(fullStream);
            var data = new T();
            using (var stream = rawData.CreateBinaryReader())
            {
                data.ReadData(stream, _userdata.IsValueCreated ? _userdata.Value : EMPTY_USERDATA, (int) _actualSize);
            }

            if (rawData.Length > IMMEDIATELY_GC_THRESHOLD)
            {
                rawData = null;
                GC.Collect();
            }

            data.TypeGuid = TypeGuid;

            return data;
        }

    private byte[] GetRawData(BinaryReader stream)
    {
            byte[]? rawData;
            if (!stream.BaseStream.CanSeek)
                throw new IOException("The base stream must support random access (seek)");
            stream.BaseStream.Seek((long) _offset, SeekOrigin.Begin);
            switch (_storageFormat)
            {
                case StorageFormat.Uncompressed:
                {
                    rawData = stream.ReadBytes((int) _storageSize);
                    break;
                }
                case StorageFormat.LZ4HC:
                {
                    rawData = stream.ReadBytes((int) _storageSize);
                    rawData = LZ4Decompress(rawData, (int) _actualSize);
                    if (rawData.Length > IMMEDIATELY_GC_THRESHOLD)
                        GC.Collect();
                    break;
                }
                default:
                    throw new ArgumentException("Unsupported data storage format: " + _storageFormat);
            }

            return rawData;
        }

    public T GetData() => ReadData();

    private static byte[] LZ4Decompress(byte[] input, int outLength) => LZ4Codec.Decode(input, 0, input.Length, outLength);
}