using System.Linq;
using NAPS2.EtoForms.Desktop;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Images;

public class DesktopImagesControllerTests : ContextualTests
{
    [Fact]
    public void ReceiveScannedImage_AutoSelectsNewImage_WhenNothingSelected()
    {
        var imageList = new UiImageList();
        var controller = new DesktopImagesController(imageList);
        var callback = controller.ReceiveScannedImage();

        callback(CreateScannedImage());

        Assert.Single(imageList.Images);
        Assert.Single(imageList.Selection);
        Assert.Equal(imageList.Images[0], imageList.Selection.First());
    }

    [Fact]
    public void ReceiveScannedImage_DoesNotChangeSelection_WhenAlreadySelected()
    {
        var imageList = new UiImageList();
        var controller = new DesktopImagesController(imageList);
        var existingImage = new UiImage(CreateScannedImage());
        imageList.Mutate(new ListMutation<UiImage>.Append(existingImage));
        imageList.UpdateSelection(ListSelection.Of(existingImage));

        var callback = controller.ReceiveScannedImage();
        callback(CreateScannedImage());

        Assert.Equal(2, imageList.Images.Count);
        Assert.Single(imageList.Selection);
        Assert.Equal(existingImage, imageList.Selection.First());
    }
}
