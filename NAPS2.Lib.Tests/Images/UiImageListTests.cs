using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

// TODO: Add more tests
public class UiImageListTests : ContextualTests
{
    [Fact]
    public void HasUnsavedChanges_EmptyList()
    {
        var list = new UiImageList();
        Assert.False(list.HasUnsavedChanges);
    }

    [Fact]
    public void HasUnsavedChanges_SingleImage()
    {
        var list = new UiImageList();
        var item = new UiImage(CreateScannedImage());

        // Add an image
        list.Mutate(new ListMutation<UiImage>.Append(item));
        Assert.True(list.HasUnsavedChanges);

        // Mark the image as saved
        var listState1 = list.CurrentState;
        var itemState1 = item.GetClonedImage();
        list.MarkSaved(listState1, new[] { itemState1 });
        Assert.False(list.HasUnsavedChanges);

        // Modify the image
        item.AddTransform(new RotationTransform(90));
        Assert.True(list.HasUnsavedChanges);

        // Mark the image as saved but with a stale image state
        var listState2 = list.CurrentState;
        list.MarkSaved(listState2, new[] { itemState1 });
        Assert.True(list.HasUnsavedChanges);

        // Mark the image as saved but with a stale list state
        var itemState2 = item.GetClonedImage();
        list.MarkSaved(listState1, new[] { itemState2 });
        Assert.True(list.HasUnsavedChanges);

        // Mark the image as saved with fresh states
        list.MarkSaved(listState2, new[] { itemState2 });
        Assert.False(list.HasUnsavedChanges);
    }

    [Fact]
    public void HasUnsavedChanges_MultipleImages()
    {
        var list = new UiImageList();
        var item1 = new UiImage(CreateScannedImage());
        var item2 = new UiImage(CreateScannedImage());

        // Add images
        list.Mutate(new ListMutation<UiImage>.Append(item1));
        list.Mutate(new ListMutation<UiImage>.Append(item2));
        Assert.True(list.HasUnsavedChanges);

        // Mark the first image as saved
        var listState1 = list.CurrentState;
        list.MarkSaved(listState1, new[] { item1.GetClonedImage() });
        Assert.True(list.HasUnsavedChanges);

        // Mark the second image as saved
        list.MarkSaved(listState1, new[] { item2.GetClonedImage() });
        Assert.False(list.HasUnsavedChanges);

        // Swap the images
        list.Mutate(new ListMutation<UiImage>.MoveUp(), ListSelection.Of(item2));
        Assert.True(list.HasUnsavedChanges);

        // Mark something as saved again - we can't really differentiate better than that
        var listState2 = list.CurrentState;
        list.MarkSaved(listState2, new[] { item1.GetClonedImage() });
        Assert.False(list.HasUnsavedChanges);

        // Modify one image
        item1.AddTransform(new RotationTransform(90));
        Assert.True(list.HasUnsavedChanges);

        // Mark the other image as saved
        var listState3 = list.CurrentState;
        list.MarkSaved(listState3, new[] { item2.GetClonedImage() });
        Assert.True(list.HasUnsavedChanges);

        // Mark the modified image as saved
        list.MarkSaved(listState3, new[] { item1.GetClonedImage() });
        Assert.False(list.HasUnsavedChanges);
    }

    [Fact]
    public void HasUnsavedChanges_MarkAllSaved()
    {
        var list = new UiImageList();
        var item1 = new UiImage(CreateScannedImage());
        var item2 = new UiImage(CreateScannedImage());

        list.Mutate(new ListMutation<UiImage>.Append(item1));
        list.Mutate(new ListMutation<UiImage>.Append(item2));
        Assert.True(list.HasUnsavedChanges);

        list.MarkAllSaved();
        Assert.False(list.HasUnsavedChanges);
    }
}