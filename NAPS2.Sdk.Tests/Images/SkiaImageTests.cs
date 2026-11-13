using System.Buffers.Binary;
using System.Text;
using NAPS2.Images.Skia;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class SkiaImageTests
{
    [Fact]
    public void SavePngWithResolutionWritesSinglePhysChunk()
    {
        var ctx = new SkiaImageContext();
        using var image = ctx.Load(ImageResources.dog);
        image.SetResolution(300, 150);

        var chunks = ReadPngChunks(image.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

        // Skia doesn't write the resolution, so we add our own pHYs chunk. If Skia ever writes one too, there would
        // be two, which isn't valid (even though decoders generally tolerate it).
        var physChunk = Assert.Single(chunks, chunk => chunk.Type == "pHYs");
        Assert.Equal("IHDR", chunks[0].Type);
        Assert.Equal("pHYs", chunks[1].Type);
        Assert.Equal(9, physChunk.Data.Length);
        // The resolution is stored as pixels per meter
        Assert.Equal(11811u, BinaryPrimitives.ReadUInt32BigEndian(physChunk.Data));
        Assert.Equal(5906u, BinaryPrimitives.ReadUInt32BigEndian(physChunk.Data.AsSpan(4)));
        Assert.Equal(1, physChunk.Data[8]);
    }

    [Fact]
    public void SavePngWithoutResolutionWritesNoPhysChunk()
    {
        var ctx = new SkiaImageContext();
        using var image = ctx.Load(ImageResources.dog);
        image.SetResolution(0, 0);

        var chunks = ReadPngChunks(image.SaveToMemoryStream(ImageFileFormat.Png).ToArray());

        // If this fails then Skia has started writing its own pHYs chunk, and we need to stop adding a second one
        Assert.DoesNotContain(chunks, chunk => chunk.Type == "pHYs");
        Assert.Equal("IHDR", chunks[0].Type);
    }

    private static List<(string Type, byte[] Data)> ReadPngChunks(byte[] png)
    {
        var chunks = new List<(string, byte[])>();
        // Each chunk is a length (4 bytes), type (4 bytes), data, and CRC (4 bytes), after the 8-byte file signature
        int offset = 8;
        while (offset < png.Length)
        {
            int length = (int) BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset));
            var type = Encoding.ASCII.GetString(png, offset + 4, 4);
            chunks.Add((type, png.AsSpan(offset + 8, length).ToArray()));
            offset += 12 + length;
        }
        return chunks;
    }
}
