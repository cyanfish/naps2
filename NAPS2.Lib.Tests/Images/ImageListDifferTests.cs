using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class ImageListDifferTests : ContextualTests
{
    private readonly UiImageList _imageList;
    private readonly ImageListDiffer _differ;

    public ImageListDifferTests()
    {
        _imageList = new UiImageList();
        _differ = new ImageListDiffer(_imageList);
    }

    [Fact]
    public void DiffEmptyList()
    {
        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Empty(diffs.ReplaceOperations);
        Assert.Empty(diffs.TrimOperations);
        Assert.False(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffAppendOne()
    {
        var image1 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Single(diffs.AppendOperations);
        Assert.Equal(image1, diffs.AppendOperations[0].Item);
        Assert.Empty(diffs.ReplaceOperations);
        Assert.Empty(diffs.TrimOperations);
        Assert.True(diffs.HasAnyDiff);
    }
    
    [Fact]
    public void DiffNoChangesToOne()
    {
        var image1 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));
        _differ.GetAndFlushDiffs();

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Empty(diffs.ReplaceOperations);
        Assert.Empty(diffs.TrimOperations);
        Assert.False(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffReplaceOne()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));
        _differ.GetAndFlushDiffs();
        _imageList.Mutate(new ListMutation<UiImage>.ReplaceWith(image2), ListSelection.Of(image1));

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Single(diffs.ReplaceOperations);
        Assert.Equal(0, diffs.ReplaceOperations[0].Index);
        Assert.Equal(image2, diffs.ReplaceOperations[0].Item);
        Assert.Empty(diffs.TrimOperations);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffTrimOne()
    {
        var image1 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));
        _differ.GetAndFlushDiffs();
        _imageList.Mutate(new ListMutation<UiImage>.DeleteAll());

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Empty(diffs.ReplaceOperations);
        Assert.Single(diffs.TrimOperations);
        Assert.Equal(1, diffs.TrimOperations[0].Count);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffAddTransform()
    {
        var image1 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));
        _differ.GetAndFlushDiffs();
        image1.AddTransform(new BrightnessTransform(300));

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Single(diffs.ReplaceOperations);
        Assert.Equal(0, diffs.ReplaceOperations[0].Index);
        Assert.Equal(image1, diffs.ReplaceOperations[0].Item);
        Assert.Empty(diffs.TrimOperations);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffSetThumbnail()
    {
        var image1 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1));
        _differ.GetAndFlushDiffs();
        image1.SetThumbnail(new GdiImage(new Bitmap(100, 100)), TransformState.Empty);

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Single(diffs.ReplaceOperations);
        Assert.Equal(0, diffs.ReplaceOperations[0].Index);
        Assert.Equal(image1, diffs.ReplaceOperations[0].Item);
        Assert.Empty(diffs.TrimOperations);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffMultipleChangesWithAppend()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());
        var image3 = new UiImage(CreateScannedImage());
        var image4 = new UiImage(CreateScannedImage());
        var image5 = new UiImage(CreateScannedImage());
        var image6 = new UiImage(CreateScannedImage());
        var image7 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1, image2, image3, image4, image5));
        _differ.GetAndFlushDiffs();
        _imageList.Mutate(new ListMutation<UiImage>.MoveDown(), ListSelection.Of(image1));
        _imageList.Mutate(new ListMutation<UiImage>.ReplaceWith(image6), ListSelection.Of(image3));
        image4.AddTransform(new BrightnessTransform(300));
        _imageList.Mutate(new ListMutation<UiImage>.Append(image7));

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Single(diffs.AppendOperations);
        Assert.Equal(image7, diffs.AppendOperations[0].Item);
        Assert.Equal(4, diffs.ReplaceOperations.Count);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 0 && x.Item == image2);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 1 && x.Item == image1);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 2 && x.Item == image6);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 3 && x.Item == image4);
        Assert.Empty(diffs.TrimOperations);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffMultipleChangesWithTrim()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());
        var image3 = new UiImage(CreateScannedImage());
        var image4 = new UiImage(CreateScannedImage());
        var image5 = new UiImage(CreateScannedImage());
        var image6 = new UiImage(CreateScannedImage());
        var image7 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1, image2, image3, image4, image5, image6));
        _differ.GetAndFlushDiffs();
        _imageList.Mutate(new ListMutation<UiImage>.MoveDown(), ListSelection.Of(image1));
        _imageList.Mutate(new ListMutation<UiImage>.ReplaceWith(image7), ListSelection.Of(image3));
        image4.AddTransform(new BrightnessTransform(300));
        _imageList.Mutate(new ListMutation<UiImage>.DeleteSelected(), ListSelection.Of(image5, image6));

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Equal(4, diffs.ReplaceOperations.Count);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 0 && x.Item == image2);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 1 && x.Item == image1);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 2 && x.Item == image7);
        Assert.Contains(diffs.ReplaceOperations, x => x.Index == 3 && x.Item == image4);
        Assert.Single(diffs.TrimOperations);
        Assert.Equal(2, diffs.TrimOperations[0].Count);
        Assert.True(diffs.HasAnyDiff);
    }

    [Fact]
    public void DiffTrimAll()
    {
        var image1 = new UiImage(CreateScannedImage());
        var image2 = new UiImage(CreateScannedImage());
        var image3 = new UiImage(CreateScannedImage());
        _imageList.Mutate(new ListMutation<UiImage>.Append(image1, image2, image3));
        _differ.GetAndFlushDiffs();
        _imageList.Mutate(new ListMutation<UiImage>.DeleteAll());

        var diffs = _differ.GetAndFlushDiffs();
        Assert.Empty(diffs.AppendOperations);
        Assert.Empty(diffs.ReplaceOperations);
        Assert.Single(diffs.TrimOperations);
        Assert.Equal(3, diffs.TrimOperations[0].Count);
        Assert.True(diffs.HasAnyDiff);
    }
}