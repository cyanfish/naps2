namespace NAPS2.Images.Bitwise;

internal static class BitwisePrimitives
{
    public static unsafe void Invert(BitwiseImageData data, int partStart = -1, int partEnd = -1)
    {
        if (partStart == -1) partStart = 0;
        if (partEnd == -1) partEnd = data.h;

        var longCount = data.stride / 8;
        var remainingStart = longCount * 8;

        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            var rowL = (long*) row;
            // Use long operations as much as possible for speed
            for (int j = 0; j < longCount; j++)
            {
                var ptr = rowL + j;
                var value = *ptr;
                *ptr = ~value;
            }
            for (int j = remainingStart; j < data.stride; j++)
            {
                var ptr = row + j;
                var value = *ptr;
                *ptr = (byte) (~value & 0xFF);
            }
        }
    }

    public static unsafe void Fill(BitwiseImageData data, byte value, int partStart = -1, int partEnd = -1)
    {
        if (partStart == -1) partStart = 0;
        if (partEnd == -1) partEnd = data.h;

        var longCount = data.stride / 8;
        var remainingStart = longCount * 8;

        long valueL = 0;
        for (int i = 0; i < 8; i++)
        {
            valueL = (valueL << 8) | value;
        }

        for (int i = partStart; i < partEnd; i++)
        {
            var row = data.ptr + data.stride * i;
            var rowL = (long*) row;
            // Use long operations as much as possible for speed
            for (int j = 0; j < longCount; j++)
            {
                var ptr = rowL + j;
                *ptr = valueL;
            }
            for (int j = remainingStart; j < data.stride; j++)
            {
                var ptr = row + j;
                *ptr = value;
            }
        }
    }
}