using System.Buffers.Binary;

namespace NAPS2.Util;

internal class EndianReader
{
    private readonly bool _reverseEndianness;

    public EndianReader(bool reverseEndianness)
    {
        _reverseEndianness = reverseEndianness;
    }

    public int ReadInt16(byte[] buf, int offset)
    {
        var value = BitConverter.ToInt16(buf, offset);
        if (_reverseEndianness) value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }

    public int ReadInt32(byte[] buf, int offset)
    {
        var value = BitConverter.ToInt32(buf, offset);
        if (_reverseEndianness) value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }
}