using System.Collections.Immutable;
using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageSerializationTests : ContextualTexts
{
    private const double TOLERANCE = 0.0001;

    [Fact]
    public void TransformSerialization()
    {
        var serializer = new XmlSerializer<List<Transform>>();
        var original = new List<Transform>
        {
            new BlackWhiteTransform(1),
            new BrightnessTransform(2),
            new ContrastTransform(3),
            new CropTransform(4,5,6,7,8,9),
            new HueTransform(10),
            new RotationTransform(11.1),
            new SaturationTransform(12),
            new ScaleTransform(13.1),
            new SharpenTransform(14),
            new ThumbnailTransform(15),
            new TrueContrastTransform(16)
        };

        var doc = serializer.SerializeToXDocument(original);
        var copy = serializer.DeserializeFromXDocument(doc);

        Assert.Equal(original.Count, copy.Count);
        Assert.True(copy[0] is BlackWhiteTransform bwt && bwt.Threshold == 1);
        Assert.True(copy[1] is BrightnessTransform bt && bt.Brightness == 2);
        Assert.True(copy[2] is ContrastTransform ct && ct.Contrast == 3);
        Assert.True(copy[3] is CropTransform crt && crt.Left == 4 && crt.Right == 5 && crt.Top == 6 && crt.Bottom == 7 && crt.OriginalWidth == 8 && crt.OriginalHeight == 9);
        Assert.True(copy[4] is HueTransform ht && ht.HueShift == 10);
        Assert.True(copy[5] is RotationTransform rt && Math.Abs(rt.Angle - 11.1) < TOLERANCE);
        Assert.True(copy[6] is SaturationTransform st && st.Saturation == 12);
        Assert.True(copy[7] is ScaleTransform sct && Math.Abs(sct.ScaleFactor - 13.1) < TOLERANCE);
        Assert.True(copy[8] is SharpenTransform sht && sht.Sharpness == 14);
        Assert.True(copy[9] is ThumbnailTransform tt && tt.Size == 15);
        Assert.True(copy[10] is TrueContrastTransform tct && tct.Contrast == 16);
    }

    [Fact]
    public void SerializeImage()
    {
        // TODO: Rethink this initialization and recovery
        using var sourceContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "source")));
        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = sourceContext.CreateProcessedImage(
            new GdiImage(new Bitmap(100, 100)),
            BitDepth.Grayscale,
            true,
            -1,
            new []{ new BrightnessTransform(100) });
        var sourceFilePath = Assert.IsType<ImageFileStorage>(sourceImage.Storage).FullPath;

        var serializedImage = SerializedImageHelper.Serialize(sourceImage, new SerializedImageHelper.SerializeOptions());
        using var destImage = SerializedImageHelper.Deserialize(destContext, serializedImage, new SerializedImageHelper.DeserializeOptions());
        var destFilePath = Assert.IsType<ImageFileStorage>(destImage.Storage).FullPath;
            
        // Backing file should be copied
        Assert.NotEqual(sourceFilePath, destFilePath);
        Assert.Equal(File.ReadAllBytes(sourceFilePath), File.ReadAllBytes(destFilePath));
        // Metadata should be (mostly) copied
        Assert.Single(destImage.TransformState.Transforms);
        Assert.Equal(100, Assert.IsType<BrightnessTransform>(destImage.TransformState.Transforms[0]).Brightness);
        Assert.True(destImage.Metadata.Lossless);
        Assert.Equal(BitDepth.Grayscale, destImage.Metadata.BitDepth);
    }

    // TODO: Add tests for other serialization cases (recovery/file/memory, serialize/deserialize options)
}